using System;

namespace TCPServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var serverManager = ServerLogic.ServerManager.GetInstance();
            serverManager.Init();
            while (true)
            {
                serverManager.Update();
            }
        }
    }
}
