using System;
using SuperOne.Network;
using UnityEngine;
using WebSocketSharp;

public class WsClient
{
    WebSocket _ws;
    public WebSocket WS => _ws;
    public bool IsAlive => (bool)(_ws?.ReadyState == WebSocketState.Open);

    public WsClient(string url, EventHandler<WebSocketSharp.ErrorEventArgs> Ws_OnError, EventHandler<WebSocketSharp.MessageEventArgs> Ws_OnMessage, EventHandler Ws_OnOpen, EventHandler<WebSocketSharp.CloseEventArgs> Ws_OnClose)
    {
        _ws = MakeConnection(url, Ws_OnError, Ws_OnMessage, Ws_OnOpen, Ws_OnClose);
    }

    private WebSocket MakeConnection(string url, EventHandler<WebSocketSharp.ErrorEventArgs> Ws_OnError, EventHandler<WebSocketSharp.MessageEventArgs> Ws_OnMessage, EventHandler Ws_OnOpen, EventHandler<WebSocketSharp.CloseEventArgs> Ws_OnClose)
    {

        var ws = new WebSocket(url);// "wss://quickdev2.super.one/ws/");// new string[] { "TCP/IP" });
        ws.OnError += Ws_OnError;
        ws.OnOpen += Ws_OnOpen;
        ws.OnMessage += Ws_OnMessage;
        ws.OnClose += Ws_OnClose;
        ws.EmitOnPing = true;

        return ws;
    }


    public void Connect()
    {

        if (_ws != null)
            _ws.ConnectAsync();

    }
    public void Disconnect()
    {
        if (_ws != null && _ws.ReadyState == WebSocketState.Open)
            _ws.Close(CloseStatusCode.Normal);
    }
    public void Send(string data)
    {
        _ws.Send(data);// status => { Debug.Log($"Sent Status: {status}, Data: {data}"); });
    }

    public void Ping(string message = null)
    {
        if (string.IsNullOrEmpty(message))
        {
            if (_ws.Ping())
            {
                Debug.Log("Pinged");
            }
        }
        else
            if (_ws.Ping(message))
        {
            //Dispatcher.RunOnMainThread(() => Debug.Log("Pinged"));
        }
    }

}