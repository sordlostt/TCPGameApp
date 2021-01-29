using System;
using System.Collections.Generic;
using System.Text;

namespace ClientLogic
{
    public class ClientManager
    {
        private static ClientManager _instance;

        public static ClientManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ClientManager();
            }

            return _instance;
        }

        private ClientManager()
        {

        }

        enum GameState
        {
            IN_LOBBY,
            WAITING_FOR_HOST,
            PLAYING,
            END
        }

        GameState gamestate;

        TCPClientLibrary.Client client;
        Player player;
        bool stateInitDone = false;

        public void Init()
        {
            player = new Player();
            client.Start();
            gamestate = GameState.IN_LOBBY;
        }

        public void Update()
        {
            switch (gamestate)
            {
                case GameState.IN_LOBBY:
                    if (!stateInitDone)
                    {
                        client.socket.MessageReceived += ProcessServerMessage;
                        stateInitDone = true;
                    }
                    break;

                case GameState.WAITING_FOR_HOST:
                    if (player.isHost)
                    {
                        PromptInput("Type START to start the game.");
                    }    
                    break;

                case GameState.PLAYING:
                    break;

                case GameState.END:
                    break;
            }
        }

        private void PromptInput(string promptMessage)
        {
            Console.WriteLine(promptMessage);
            var input = Console.ReadLine();
            ProcessInput(input);
        }

        private void ProcessInput(string input)
        {
            var message = input.Split(';');

            switch (message[0])
            {
                case "START":
                    client.Send($"{message[0]}<EOF>");
                    break;

                case "ANSWER":
                    client.Send($"");
                    break;
            }
        }

        private void ProcessServerMessage(string message)
        {
            var messageSplit = message.Split(';');
            string operation = messageSplit[0];

            switch (operation)
            {
                case "ID":
                    player.id = Int32.Parse(messageSplit[1]);
                    break;
                case "HOST":
                    player.isHost = true;
                    break;
                case "FREEZE":
                    player.isFrozen = true;
                    break;


                case "START":
                    break;

                case "NEXT":
                    break;

                case "QUESTION":
                    break;

                case "RIGHT":
                    break;

                case "WRONG":
                    break;
            }
        }
    }
}
