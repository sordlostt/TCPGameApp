﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;

namespace TCPClientLibrary
{
        public class StateObject
        {
            public Socket workSocket = null;
            public const int BufferSize = 256;
            public byte[] buffer = new byte[BufferSize]; 
            public StringBuilder sb = new StringBuilder();
        }

        public class AsynchronousClientSocket
        {
            private static ManualResetEvent connectDone =
                new ManualResetEvent(false);
            private static ManualResetEvent sendDone =
                new ManualResetEvent(false);
            private static ManualResetEvent receiveDone =
                new ManualResetEvent(false);

            private static String response = String.Empty;

            public void Start(IPAddress ipAddress, int port)
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                    Socket client = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);
 
                    client.BeginConnect(remoteEP,
                        new AsyncCallback(ConnectCallback), client);
                    connectDone.WaitOne();
 
                    Send(client, "This is a test<EOF>");
                    sendDone.WaitOne();

                    Receive(client);
                    receiveDone.WaitOne();

                    Console.WriteLine("Response received : {0}", response);

                    client.Shutdown(SocketShutdown.Both);
                    client.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            private void ConnectCallback(IAsyncResult ar)
            {
                try
                {
                    Socket client = (Socket)ar.AsyncState;

                    client.EndConnect(ar);

                    Console.WriteLine("Socket connected to {0}",
                        client.RemoteEndPoint.ToString());

                    connectDone.Set();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            private void Receive(Socket client)
            {
                try
                {
                    StateObject state = new StateObject();
                    state.workSocket = client;
 
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            private void ReceiveCallback(IAsyncResult ar)
            {
                try
                {
                    StateObject state = (StateObject)ar.AsyncState;
                    Socket client = state.workSocket;

                    int bytesRead = client.EndReceive(ar);

                    if (bytesRead > 0)
                    { 
                        state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReceiveCallback), state);
                    }
                    else
                    {
                        if (state.sb.Length > 1)
                        {
                            response = state.sb.ToString();
                        } 
                        receiveDone.Set();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            private void Send(Socket client, String data)
            {
                byte[] byteData = Encoding.ASCII.GetBytes(data);
 
                client.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), client);
            }

            private void SendCallback(IAsyncResult ar)
            {
                try
                {
                    Socket client = (Socket)ar.AsyncState;
 
                    int bytesSent = client.EndSend(ar);
                    Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                    sendDone.Set();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
