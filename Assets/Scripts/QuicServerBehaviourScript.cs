using System;
﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

using Quiche;

public class QuicServerBehaviourScript : MonoBehaviour
{
    private static class NativeMethods
    {
        /* negotiate version */
        [DllImport("libquiche")]
        internal static extern long /* ssize_t */ quiche_negotiate_version(
            byte[] /* *const u8 */ scid,
            ulong /* size_t */ scid_len,
            byte[] /* *const u8 */ dcid,
            ulong /* size_t */ dcid_len,
            byte[] /* *mut u8 */ _out,
            ulong /* size_t */ out_len);

        /* version supported */
        [DllImport("libquiche")]
        internal static extern bool quiche_version_is_supported(
            uint version);

        /* retry */
        [DllImport("libquiche")]
        internal static extern long /* ssize_t */ quiche_retry(
            byte[] /* *const u8 */ scid,
            ulong /* size_t */ scid_len,
            byte[] /* *const u8 */ dcid,
            ulong /* size_t */ dcid_len,
            byte[] /* *const u8 */ new_scid,
            ulong /* size_t */ new_scid_len,
            byte[] /* *const u8 */ token,
            ulong /* size_t */ tolen_len,
            byte[] /* *mut u8 */ _out,
            ulong /* size_t */ out_len);
    }

    private class PartialResponse
    {
        public byte[] Body { get; }
        public int Written { get; set; }
        public PartialResponse(byte[] body, int written)
        {
            Body = body;
            Written = written;
        }
    }

    private class Client
    {
        public IPEndPoint RemoteIpEndPoint { get; }
        public QuicheConnection Connection { get; }
        public Dictionary<ulong, PartialResponse> PartialResponses { get; }

        public Client(QuicheConnection conn, IPEndPoint remoteIpEndPoint)
        {
            RemoteIpEndPoint = remoteIpEndPoint;
            Connection = conn;
            PartialResponses = new Dictionary<ulong, PartialResponse>();
        }
    }

    private class ClientState
    {
        public byte[] ReceiveBytes { get; set; }
        public IPEndPoint remoteIpEndPoint;
        public ClientState()
        {
            remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }
    }

    public string localIpString = "127.0.0.1";
    public int localPort = 4433;
    public bool earlyData = false;
    public bool noRetry = true;

    private UdpClient udpClient = null;
    private QuicheListener quiche = null;

    private IAsyncResult receiveResult = null;

    private Dictionary<string, Client> clients = new Dictionary<string, Client>();

    private byte[] negotiateBuf = new byte[65535];
    private byte[] streamRecv = new byte[65535];
    private byte[] sendBuf = new byte[QuicheClient.MAX_DATAGRAM_SIZE];

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(QuicheVersion.GetVersion());
        QuicheDebug.EnableDebugLogging((line, argp) => {
            Debug.Log(line);
        });
        var localAddress = IPAddress.Parse(localIpString);
        var localEP = new IPEndPoint(localAddress, localPort);
        udpClient = new UdpClient(localEP);
        var config = new QuicheConfig(QuicheVersion.QUICHE_PROTOCOL_VERSION);
        byte[] protos = Encoding.ASCII.GetBytes("\x05hq-24\x05hq-23\x08http/0.9");
        config.LoadCertChainFromPemFile($"{Application.dataPath}/Server/cert.crt");
        config.LoadPrivKeyFromPemFile($"{Application.dataPath}/Server/cert.key");
        config.SetApplicationProtos(protos);
        config.SetIdleTimeout(5000);
        config.SetMaxPacketSize(QuicheClient.MAX_DATAGRAM_SIZE);
        config.SetInitialMaxData(10000000);
        config.SetInitialMaxStreamDataBidiLocal(1000000);
        config.SetInitialMaxStreamDataBidiRemote(1000000);
        config.SetInitialMaxStreamDataUni(1000000);
        config.SetInitialMaxStreamsBidi(100);
        config.SetInitialMaxStreamsUni(100);
        config.SetDisableActiveMigration(true);
        if(earlyData){
            config.EnableEarlyData();
        }
        quiche = new QuicheListener(config);
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var client in clients.Values)
        {
            client.Connection.OnTimeout();
        }
        if(receiveResult == null)
        {
            receiveResult = udpClient.BeginReceive((res) => {
                var state = (ClientState)res.AsyncState;
                state.ReceiveBytes = udpClient.EndReceive(res, ref state.remoteIpEndPoint);
            }, new ClientState());
        }
        if(receiveResult.IsCompleted)
        {
            Poll((ClientState)receiveResult.AsyncState);
            receiveResult = null;
        }
        foreach(var key in clients.Keys)
        {
            var client = clients[key];
            var written = client.Connection.Send(sendBuf);
            if(written == (int)QuicheError.QUICHE_ERR_DONE)
            {
                Debug.Log("done writing");
                continue;
            }
            if(written < 0)
            {
                QuicheError err = (QuicheError)Enum
                    .ToObject(typeof(QuicheError), written);
                Debug.LogError($"ailed to create packet: {written} {err}");
                client.Connection.Close(false, 0x1, Encoding.ASCII.GetBytes("fail"));
                continue;
            }
            var buf = sendBuf.Take(written).ToArray();
            udpClient.Send(buf, written, client.RemoteIpEndPoint);
            Debug.Log($"sent {written} bytes");
        }
        foreach(var key in clients.Keys
            .Where(k => clients[k].Connection.IsClosed).ToArray())
        {
            Debug.LogWarning("Connection Dispose");
            var client = clients[key];
            clients.Remove(key);
            client.Connection.Dispose();
        }
    }

    private byte[] MintToken(QuicheHeaderInfo header, IPEndPoint src)
    {
        var token = Encoding.ASCII.GetBytes("quiche");
        var addr = src.Address.GetAddressBytes();
        return token.Concat(addr).Concat(header.Dcid).ToArray();
    }

    private byte[] ValidateToken(byte[] token, IPEndPoint src)
    {
        if(token.Length < 6)
        {
            return null;
        }

        if(Encoding.ASCII.GetString(token.Take(6).ToArray()) != "quiche")
        {
            throw new Exception();
        }
        var buf = token.Skip(6).ToArray();

        var addr = src.Address.GetAddressBytes();
        if(token.Length < addr.Length || !Enumerable.SequenceEqual(buf.Take(addr.Length).ToArray(), addr))
        {
            return null;
        }
        return buf.Skip(addr.Length).ToArray();
    }

    private static long NegotiateVersion(byte[] scid, byte[] dcid, byte[] _out)
    {
        return NativeMethods.quiche_negotiate_version(
            scid, (ulong)scid.Length,
            dcid, (ulong)dcid.Length,
            _out, (ulong)_out.Length);
    }

    private static bool VersionIsSupported(uint version)
    {
        return NativeMethods.quiche_version_is_supported(version);
    }

    private long Retry(
        byte[] scid,
        byte[] dcid,
        byte[] new_scid,
        byte[] token,
        byte[] _out)
    {
        return NativeMethods.quiche_retry(
            scid, (ulong)scid.Length,
            dcid, (ulong)dcid.Length,
            new_scid, (ulong)new_scid.Length,
            token, (ulong)token.Length,
            _out, (ulong)_out.Length);
    }

    private void Poll(ClientState state)
    {
        var recvBytes = state.ReceiveBytes;
        var remoteIpEndPoint = state.remoteIpEndPoint;
        QuicheHeaderInfo header;
        try{
            header = QuicheHeaderInfo.Construct(recvBytes);
        } catch(Exception e)
        {
            Debug.LogError($"{e}");
            return;
        }

        if(header.Type == 6 /* Type::VersionNegotiation */)
        {
            Debug.LogError("Version negotiation invalid on the server");
            return;
        }

        var key = HexDump(header.Dcid);

        Client client;
        if(!clients.TryGetValue(key, out client))
        {
            if(header.Type != 1 /* Type::Initial */)
            {
                Debug.LogError("Packet is not Initial");
                return;
            }

            if(!VersionIsSupported(header.Version))
            {
                Debug.LogWarning("Doing version negotiation");

                var length = (int)NegotiateVersion(
                    header.Scid, header.Dcid, negotiateBuf);
                var sendBuf = negotiateBuf.Take(length).ToArray();
                udpClient.Send(sendBuf, length, remoteIpEndPoint);
                return;
            }
            var scid = new byte[QuicheClient.LOCAL_CONN_ID_LEN];
            new System.Random().NextBytes(scid);
            byte[] odcid = new byte[65535];
            if(!noRetry)
            {
                if(header.Token.Length == 0)
                {
                    Debug.LogWarning("Doing stateless retry");

                    Debug.Log($"Retry: scid={HexDump(header.Scid)} new_scid={HexDump(scid)}");

                    var new_token = MintToken(header, remoteIpEndPoint);
                    var _out = new byte[65535];
                    var written = (int)Retry(header.Scid, header.Dcid, scid, new_token, _out);
                    udpClient.Send(
                        _out.Take(written).ToArray(), written, remoteIpEndPoint);
                    return;
                }
                odcid = ValidateToken(header.Token, remoteIpEndPoint);
                if(odcid == null)
                {
                    Debug.LogError("Invalid address validation token");
                    return;
                }
            }
            if(scid.Length != header.Dcid.Length)
            {
                Debug.LogError("Invalid destination connection ID");
                return;
            }
            scid = header.Dcid;
            var conn = quiche.Accept(scid, odcid);
            var _client = new Client(conn, remoteIpEndPoint);
            clients.Add(HexDump(scid), _client);
            client = _client;
        }
        var read = client.Connection.Receive(recvBytes);
        if(read == (int)QuicheError.QUICHE_ERR_DONE)
        {
            Debug.Log("done reading");
            return;
        }
        if(read < 0)
        {
            QuicheError err = (QuicheError)Enum
                .ToObject(typeof(QuicheError), read);
            Debug.LogError($"recv failed {err}");
            throw new Exception();
        }
        Debug.Log($"recv {read} bytes");
        if(client.Connection.IsInEarlyData || client.Connection.IsEstablished)
        {
            foreach(ulong streamId in client.Connection.Writable())
            {
                if(!client.PartialResponses.ContainsKey(streamId))
                {
                    continue;
                }
                var partialResponse = client.PartialResponses[streamId];
                var written = client.Connection.StreamSend(streamId, partialResponse.Body, true);
                if(written < 0)
                {
                    continue;
                }
                partialResponse.Written += written;
                if(partialResponse.Written == partialResponse.Body.Length)
                {
                    client.PartialResponses.Remove(streamId);
                }

            }
            foreach(ulong streamId in client.Connection.Readable())
            {
                Debug.Log($"stream {streamId} is readable");
                bool fin = false;
                var readStream = client.Connection.StreamReceive(
                    streamId, streamRecv, ref fin);
                if(readStream < 0)
                {
                    continue;
                }
                if(fin)
                {
                    var body = Encoding.ASCII.GetBytes("Hello World!\n");
                    var written = client.Connection.StreamSend(
                        streamId, body, true);
                    if(written < 0)
                    {
                        continue;
                    }
                    if(written < body.Length)
                    {
                        var partialResponse = new PartialResponse(body, written);
                        client.PartialResponses.Add(streamId, partialResponse);
                    }
                }
            }
        }
    }

    private string HexDump(byte[] buf)
    {
        return string.Join("", buf.Select(x => x.ToString("x2")).ToArray());
    }


    void OnDestroy()
    {
        foreach(var key in clients.Keys)
        {
            var client = clients[key];
            client.Connection.Dispose();
        }
        clients.Clear();
        if(quiche != null)
        {
            quiche.Dispose();
            quiche = null;
        }
        if(udpClient != null)
        {
            udpClient.Dispose();
            udpClient = null;
        }
    }
}
