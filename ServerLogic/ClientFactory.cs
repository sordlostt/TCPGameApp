using System;
using System.Collections.Generic;
using System.Text;

namespace ServerLogic
{
    public static class ClientFactory
    {
        public static Client CreateClient(int connectionID)
        {
            return new Client { id = connectionID, points = 0 };
        }
    }
}
