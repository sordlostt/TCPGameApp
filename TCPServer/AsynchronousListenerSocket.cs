using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace TCPServerLibrary
{
    public delegate void OnMessageReceived(int connectionID);

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
        public ManualResetEvent allDone = new ManualResetEvent(false);

        public OnMessageReceived PlayerConnected;
        public OnMessageReceived PlayerMessageReceived;

        public List<StateObject> openConnections;

        public int connectionsLimit;

        public string lastMessage;

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
                        allDone.Reset();
 
                        listenerSocket.BeginAccept(
                            new AsyncCallback(AcceptCallback),
                            listenerSocket);

                        allDone.WaitOne();
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
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();
            state.workSocket = handler;
            state.connectionID = new Random().Next(1000,1999);
            openConnections.Add(state);

            PlayerConnected?.Invoke(state.connectionID);

            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                { 
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);

                    lastMessage = content;
                    PlayerMessageReceived?.Invoke(state.connectionID);

                    Send(handler, content);
                }
                else
                {
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
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
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                // cos zrob
            }
        }
    }
}

