using System;
using System.Collections.Generic;
using System.Text;
using TCPServerLibrary;

namespace ServerLogic
{
    public static class ClientMessageFactory
    {
        public static void SendStartMessage(Server server, int hostID)
        {
            server.Send("START<EOF>", hostID);
        }

        public static void SendNextRoundMessage(Server server, int clientID)
        {
            server.Send("NEXT<EOF>", clientID);
        }

        public static void SendHostMessage(Server server, int clientID)
        {
            server.Send("HOST<EOF>", clientID);
        }

        public static void SendIDMessage(Server server, int clientID)
        {
            server.Send($"ID;{clientID}<EOF>", clientID);
        }

        public static void SendWrongAnswerMessage(Server server, int clientID)
        {
            server.Send("WRONG<EOF>", clientID);
        }

        public static void SendRightAnswerMessage(Server server, int clientID)
        {
            server.Send("RIGHT<EOF>", clientID);
        }

        public static void SendFreezeMessage(Server server, int clientID)
        {
            server.Send("FREEZE<EOF>", clientID);
        }

        public static void SendQuestionMessage(Server server, int clientID, string questionText, int questionID)
        {
            server.Send($"QUESTION;{questionText};{questionID}<EOF>", clientID);
        }
        
        public static void SendWinMessage(Server server, int clientID)
        {
            server.Send("WIN<EOF>", clientID);
        }

        public static void SendLoseMessage(Server server, int clientID)
        {
            server.Send("LOSE<EOF>", clientID);
        }
    }
}
