using System;
﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class QuicBehaviourScript : MonoBehaviour
{

    private UdpClient client = null;
    private Quiche quiche = null;

    private IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

    private IAsyncResult receiveResult = null;

    private byte[] buf;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Quiche.GetVersion());
        Quiche.DebugLog((line, argp) => {
            Debug.Log(line);
        });
        client = new UdpClient();
        quiche = new Quiche();
        Connect("https://127.0.0.1:4433/index.html");
    }

    // Update is called once per frame
    void Update()
    {
        Poll();
    }

    private void Connect(string url)
    {
        var uri = new Uri(url);
        var host = uri.Host;
        var port = uri.Port;
        client.Connect($"{host}", port);
        quiche.Connect(uri.Authority);
        Debug.Log(
            $"connecting to {uri.Authority} from {client.Client.LocalEndPoint} "
            + $"with scid {quiche.HexDump}");
        // initial send
        buf = new byte[Quiche.MAX_DATAGRAM_SIZE];
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
                if(read == -1)
                {
                    Debug.Log("done reading");
                    return;
                }
                if(read < 0)
                {
                    Debug.LogError($"recv failed {read}");
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
            // TODO
            return;
        }
        var write = quiche.Send(buf);
        if(write == -1)
        {
            return;
        }
        if(write < 0)
        {
            Debug.LogError($"send failed {write}");
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
