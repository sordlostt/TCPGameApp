﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace TCPServerLibrary
{
    public delegate void OnConnectionStarted(int connectionID);
    public delegate void OnConnectionEnded(int connectionID);
    public delegate void OnMessageReceived(int connectionID, string message);

    public class StateObject
    {
        public const int BufferSize = 1024;

        public byte[] buffer = new byte[BufferSize];

        public StringBuilder sb = new StringBuilder();

        public Socket workSocket = null;

        public int connectionID;
    }

    public class AsynchronousListenerSocket
    {
        public ManualResetEvent connectionDone = new ManualResetEvent(false);

        public OnConnectionStarted PlayerConnected;
        public OnConnectionEnded PlayerDisconnected;
        public OnMessageReceived PlayerMessageReceived;

        public List<StateObject> openConnections = new List<StateObject>();

        public int connectionsLimit = 4;


        public IPEndPoint localEndPoint;
        public Socket listenerSocket;

        public void Init(int port)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = hostInfo.AddressList[1];
            localEndPoint = new IPEndPoint(ipAddress, port);
            listenerSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            try
            {
                listenerSocket.Bind(localEndPoint);
                listenerSocket.Listen(4);

                while (true)
                {
                    if (openConnections.Count < connectionsLimit)
                    {
                        connectionDone.Reset();
 
                        listenerSocket.BeginAccept(
                            new AsyncCallback(AcceptCallback),
                            listenerSocket);

                        connectionDone.WaitOne();
                    }
                }

            }
            catch (Exception e)
            {
                // tu cos wklej
            }
        }

       public void AcceptCallback(IAsyncResult ar)
        {
            connectionDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();
            state.workSocket = handler;

            var newStateID = new Random().Next(1000, 1999);
            if (openConnections.Count > 0)
            {
                while (openConnections.Find(x => x.connectionID == newStateID) != null)
                {
                    newStateID = new Random().Next(1000, 1999);
                }
            }
            state.connectionID = newStateID;

            openConnections.Add(state);

            Console.WriteLine($"Client connected. Connection ID: {state.connectionID}");
            PlayerConnected?.Invoke(state.connectionID);

            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            try
            {
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    content = state.sb.ToString();
                    if (content.IndexOf("<EOF>") > -1)
                    {
                        content = content.Remove(content.IndexOf("<EOF>"));
                        Console.WriteLine($"Message from {state.connectionID}: {content}");
                        PlayerMessageReceived?.Invoke(state.connectionID, content);
                        state.buffer = new byte[StateObject.BufferSize];
                        state.sb.Clear();
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                    else
                    {
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Client disconnected. Connection ID: {state.connectionID}");
                PlayerDisconnected?.Invoke(state.connectionID);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                openConnections.Remove(state);
            }
        }

        public void Send(Socket handler, String data)
        { 
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
 
                int bytesSent = handler.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch (Exception e)
            {
                // cos zrob
            }
        }
    }
}

