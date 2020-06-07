using System;
﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using UnityEngine;

using Quiche;

public class QuicBehaviourScript : MonoBehaviour
{
    private const ulong HTTP_REQ_STREAM_ID = 4;
    private UdpClient client = null;
    private QuicheClient quiche = null;

    private IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

    private IAsyncResult receiveResult = null;

    private byte[] buf;

    private Uri uri;
    private bool req_sent = false;

    private byte[] streamRecv = new byte[65535];

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(QuicheVersion.GetVersion());
        QuicheDebug.EnableDebugLogging((line, argp) => {
            Debug.Log(line);
        });
        client = new UdpClient();

        var config = new QuicheConfig(0xbabababa);
        byte[] protos = Encoding.ASCII.GetBytes("\x05hq-24\x05hq-23\x08http/0.9");
        config.SetApplicationProtos(protos);
        config.SetIdleTimeout(5000);
        config.SetMaxPacketSize(QuicheClient.MAX_DATAGRAM_SIZE);
        config.SetInitialMaxData(10000000);
        config.SetInitialMaxStreamDataBidiLocal(1000000);
        config.SetInitialMaxStreamDataUni(1000000);
        config.SetInitialMaxStreamsBidi(100);
        config.SetInitialMaxStreamsUni(100);
        config.SetDisableActiveMigration(true);
        config.VerifyPeer(false);
        quiche = new QuicheClient(config);
        Connect("https://127.0.0.1:4433/index.html");
    }

    // Update is called once per frame
    void Update()
    {
        Poll();
    }

    private void Connect(string url)
    {
        uri = new Uri(url);
        var host = uri.Host;
        var port = uri.Port;
        client.Connect($"{host}", port);
        quiche.Connect(uri.Authority);
        Debug.Log(
            $"connecting to {uri.Authority} from {client.Client.LocalEndPoint} "
            + $"with scid {quiche.HexDump}");
        // initial send
        buf = new byte[QuicheClient.MAX_DATAGRAM_SIZE];
        int write = quiche.Send(buf);
        client.Send(buf, write);
    }

    private void Poll()
    {
        if(quiche.IsClosed)
        {
            return;
        }
        if(receiveResult == null)
        {
            receiveResult = client.BeginReceive((res) => {
                var recvBytes = client.EndReceive(res, ref RemoteIpEndPoint);
                var read = quiche.Receive(recvBytes);
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
        if(quiche.IsEstablished)
        {
            if(!req_sent)
            {
                Debug.Log($"sending HTTP request for {uri.PathAndQuery}");
                var req = Encoding.ASCII.GetBytes($"GET {uri.PathAndQuery}\r\n");
                var streamWrite = quiche.StreamSend(HTTP_REQ_STREAM_ID, req, true);
                if(streamWrite < 0)
                {
                    QuicheError err = (QuicheError)Enum
                        .ToObject(typeof(QuicheError), streamWrite);
                    Debug.LogError($"send failed {err}");
                    throw new Exception();
                }
                req_sent = true;
            }

            foreach(ulong streamId in quiche.Readable())
            {
                bool fin = false;
                var readStream = quiche.StreamReceive(
                    streamId, streamRecv, ref fin);
                if(readStream < 0)
                {
                    continue;
                }
                var res = Encoding.ASCII.GetString(
                    streamRecv.Take(readStream).ToArray());
                Debug.Log($"{res}");
                if(fin)
                {
                    var reason = Encoding.ASCII.GetBytes("kthxbye");
                    int closeError = quiche.Close(reason);
                    if(closeError < 0)
                    {
                        QuicheError err = (QuicheError)Enum
                            .ToObject(typeof(QuicheError), closeError);
                        Debug.LogError($"send failed {err}");
                        throw new Exception();
                    }
                }
            }
        }
        var write = quiche.Send(buf);
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
        client.Send(buf, write);
    }

    void OnDestroy()
    {
        if(quiche != null)
        {
            quiche.Dispose();
            quiche = null;
        }
        if(client != null)
        {
            client.Dispose();
            client = null;
        }
    }
}
