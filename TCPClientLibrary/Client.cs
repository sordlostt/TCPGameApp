using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TCPClientLibrary
{
    public class Client
    {
        private static Client _instance;

        public static Client GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Client();
            }

            return _instance;
        }

        public AsynchronousClientSocket socket;

        public void Start()
        {
            socket = new AsynchronousClientSocket();
            socket.Init(IPAddress.Parse("192.168.0.26"), 6969);
            socket.Connect();
            var socketThread = new System.Threading.Thread(socket.Receive);
            socketThread.Start();
        }

        public void Send(string message)
        {
            socket.Send(socket.state.workSocket, message);
        }
    }
}
