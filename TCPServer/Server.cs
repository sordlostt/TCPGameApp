﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TCPServerLibrary
{
    public class Server
    {
        private static Server _instance;

        public static Server GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Server();
            }

            return _instance;
        }

        private Server()
        {

        }

        public AsynchronousListenerSocket socket = new AsynchronousListenerSocket();

        Thread socketThread;

        public void Init()
        {
            socket = new AsynchronousListenerSocket();
            socket.Init(6969);
        }

        public void Start()
        {
            socketThread = new Thread(socket.Start);
            socketThread.Start();
        }

        public void Send(string message, int connectionID)
        {
            var stateObject = socket.openConnections.Find(x => x.connectionID == connectionID);
            socket.Send(stateObject, message);
        }

        public void Shutdown()
        {
            foreach (var connection in socket.openConnections)
            {
                connection.workSocket.Close();
                socket.openConnections.Remove(connection);
            }

            socketThread.Abort();
            System.Environment.Exit(0);
        }
    }
}
