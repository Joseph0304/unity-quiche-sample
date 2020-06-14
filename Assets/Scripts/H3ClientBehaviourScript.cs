using System;
﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using UnityEngine;

using Quiche;
using Quiche.H3;

public class H3ClientBehaviourScript : MonoBehaviour
{
    public string urlString = "https://127.0.0.1:4433/index.html";
    public bool isDebugLog = true;

    private IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

    // UDP Client
    private UdpClient udpClient = null;

    // QUIC Client
    private QuicheClient quicheClient = null;
    private QuicheConnection quicheConn = null;

    // HTTP3 Client
    private H3Config h3Config = null;
    private H3Connection h3Conn = null;

    private Uri uri = null;
    private byte[] buf = new byte[QuicheClient.MAX_DATAGRAM_SIZE];

    // TODO Add body request.
    private byte[] body = null;

    private IAsyncResult receiveResult = null;

    private H3Header[] req;
    private int reqsSent = 0;
    private int reqsCount = 1;
    private int reqsComplete = 0;

    // Start is called before the first frame update
    void Start()
    {
        uri = new Uri(urlString);
        Debug.Log(QuicheVersion.GetVersion());
        if(isDebugLog)
        {
            QuicheDebug.EnableDebugLogging((line, argp) => {
                Debug.Log(line);
            });
        }
        udpClient = new UdpClient();
        quicheClient = new QuicheClient(CreateQuicheConfig());
        udpClient.Connect($"{uri.Host}", uri.Port);
        quicheConn = quicheClient.Connect(uri.Authority);
        Debug.Log(
            $"connecting to {uri.Authority} from {udpClient.Client.LocalEndPoint} "
            + $"with scid {quicheClient.HexDump}");
        // initial send
        int write = quicheConn.Send(buf);
        udpClient.Send(buf, write);

        h3Config = new H3Config();
        // Prepare request.
        var reqList = new H3Header[] {
            new H3Header(":method", "GET"),
            new H3Header(":scheme", "https"),
            new H3Header(":authority", uri.Host),
            new H3Header(":path", uri.PathAndQuery),
            new H3Header("user-agent", "unity-quiche"),
        }.ToList();

        // TODO Add custom headers to the request.

        // TODO Add body
        body = null;
        if(body != null)
        {
            reqList.Add(new H3Header(
                "content-length", $"{body.Length}"));
        }
        req = reqList.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        if(quicheConn == null || quicheConn.IsClosed)
        {
            return;
        }
        quicheConn.OnTimeout();
        if(receiveResult == null)
        {
            receiveResult = udpClient.BeginReceive((res) => {
                var recvBytes = udpClient.EndReceive(res, ref remoteIpEndPoint);
                var read = quicheConn.Receive(recvBytes);
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
            }, null);
        }
        if(receiveResult.IsCompleted)
        {
            receiveResult = null;
        }
        if(quicheConn.IsEstablished && h3Conn == null)
        {
            h3Conn = new H3Connection(quicheConn, h3Config);
        }
        if(h3Conn != null)
        {
            var reqsDone = 0;

            for(var i = reqsSent; i < reqsCount; i++)
            {
                Debug.Log($"sending HTTP request [{string.Join(",", req.Select(x=>H3Header.DebugString(x)))}]");
                var streamId = h3Conn.SendRequest(req, body == null);
                if(streamId == (int)H3Error.QUICHE_H3_TRANSPORT_ERROR)
                {
                    Debug.Log("not enough stream credits, retry later...");
                    break;
                }
                if(streamId < 0)
                {
                    H3Error err = (H3Error)Enum
                        .ToObject(typeof(H3Error), streamId);
                    Debug.LogError($"recv failed {err}");
                    throw new Exception();
                }
                if(body != null)
                {
                    var e = h3Conn.SendBody((ulong)streamId, body, true);
                    if(e < 0)
                    {
                        H3Error err = (H3Error)Enum
                            .ToObject(typeof(H3Error), e);
                        Debug.LogError($"recv failed {err}");
                        throw new Exception();
                    }
                }
                reqsDone++;
            }
            reqsSent += reqsDone;
        }
        if(h3Conn != null)
        {
            H3Event ev = null;
            // Process HTTP/3 events.
            while(h3Conn != null)
            {
                var streamId = h3Conn.Poll(ref ev);
                if(streamId == (int)H3Error.QUICHE_H3_DONE)
                {
                    break;
                }
                else if(streamId < 0)
                {
                    H3Error err = (H3Error)Enum
                        .ToObject(typeof(H3Error), streamId);
                    Debug.LogError($"recv failed {err}");
                    return;
                }
                switch(ev.EventType)
                {
                    case (uint)H3EventType.QUICHE_H3_EVENT_HEADERS:
                    {
                        var rc = ev.ForEachHeader((name, nameLen, value, valueLen, argp) => {
                            Debug.Log($"got HTTP header: {name}={value}");
                        });
                        if(rc != 0)
                        {
                            Debug.LogError("failed to process headers");
                        }
                        break;
                    }
                    case (uint)H3EventType.QUICHE_H3_EVENT_DATA:
                    {
                        var _out = new byte[65535];
                        var len = h3Conn.ReceiveBody((ulong)streamId, _out);
                        if(len <= 0)
                        {
                            break;
                        }
                        Debug.Log($"{Encoding.ASCII.GetString(_out.Take((int)len).ToArray())}");
                        break;
                    }
                    case (uint)H3EventType.QUICHE_H3_EVENT_FINISHED:
                    {
                        reqsComplete++;
                        if(reqsComplete == reqsCount)
                        {
                            Debug.Log($"{reqsComplete}/{reqsCount} response(s) received, cloing...");
                            var e = quicheConn.Close(true, 0, Encoding.ASCII.GetBytes("kthxbye"));
                            h3Conn.Dispose();
                            h3Conn = null;
                            if(e == (int)QuicheError.QUICHE_ERR_DONE)
                            {
                                break;
                            }
                            else if(e < 0)
                            {
                                QuicheError err = (QuicheError)Enum
                                    .ToObject(typeof(QuicheError), e);
                                Debug.LogError($"recv failed {err}");
                                throw new Exception();
                            }
                        }
                        break;
                    }
                }
                ev.Dispose();
                ev = null;
            }
        }

        var write = quicheConn.Send(buf);
        if(write == (int)QuicheError.QUICHE_ERR_DONE)
        {
            return;
        }
        if(write < 0)
        {
            QuicheError err = (QuicheError)Enum
                .ToObject(typeof(QuicheError), write);
            Debug.LogError($"send failed {err}");
            throw new Exception();
        }
        udpClient.Send(buf, write);
    }


    void OnDestroy()
    {
        if(h3Conn != null)
        {
            h3Conn.Dispose();
            h3Conn = null;
        }
        if(h3Config != null)
        {
            h3Config.Dispose();
            h3Config = null;
        }
        if(quicheClient != null)
        {
            quicheClient.Dispose();
            quicheClient = null;
        }
        if(udpClient != null)
        {
            udpClient.Dispose();
            udpClient = null;
        }
    }

    private QuicheConfig CreateQuicheConfig()
    {
        var config = new QuicheConfig(QuicheVersion.QUICHE_PROTOCOL_VERSION);
        byte[] protos = Encoding.ASCII.GetBytes("\x05h3-24\x05h3-23");
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
        config.VerifyPeer(false);
        return config;
    }
}
