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

        public void Start()
        {
            var socket = new AsynchronousClientSocket();
            socket.Start(IPAddress.Parse("192.168.0.26"), 6969);
        }
    }
}
