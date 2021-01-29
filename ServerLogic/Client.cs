using System;
using System.Collections.Generic;
using System.Text;

namespace ServerLogic
{
    public class Client
    {
        public enum Role
            {
                HOST,
                PLAYER
            }

        public Role role;
        public int id;
        public int points;
        public int wrongAnswers;
    }
}
