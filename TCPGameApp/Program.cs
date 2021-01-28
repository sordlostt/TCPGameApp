using System;

namespace TCPGameApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TCPClientLibrary.Client();
            client.Start();
        }
    }
}
