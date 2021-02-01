using System;

namespace TCPGameApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientManager = ClientLogic.ClientManager.GetInstance();
            clientManager.Init();

            while (true)
            {
                clientManager.Update();
            }    
        }
    }
}
