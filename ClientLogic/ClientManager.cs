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

        Utils.Timer timer;

        string currentQuestionText;
        string currentQuestionID;

        public void Init()
        {
            client = new TCPClientLibrary.Client();
            client.Start();
            player = new Player();
            gamestate = GameState.IN_LOBBY;
            timer = new Utils.Timer();
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
                    if (!stateInitDone && player.isHost)
                    {
                        stateInitDone = true;
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

        private void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        private void ProcessInput(string input)
        {
            var message = input.Split(';');

            switch (message[0])
            {
                case "START":
                    client.Send($"{message[0]}<EOF>");
                    break;

                case "YES":
                    client.Send($"{player.id};ANSWER:{currentQuestionID}:YES:{timer.elapsed}<EOF>");
                    timer.raiseEvents = false;
                    break;

                case "NO":
                    client.Send($"{player.id};ANSWER:{currentQuestionID}:NO:{timer.elapsed}<EOF>");
                    timer.raiseEvents = false;
                    break;
            }
        }

        int nextEOFindex = 1;

        private void ProcessServerMessage(string message)
        {
            var messageSplit = message.Split(';');
            string operation = messageSplit[0];

            switch (operation)
            {
                case "ID":
                    DisplayMessage($"ID assigned: {messageSplit[1]}");
                    player.id = Int32.Parse(messageSplit[1]);
                    break;

                case "HOST":
                    DisplayMessage("You are now host.");
                    player.isHost = true;
                    break;

                case "FREEZE":
                    DisplayMessage("Three wrong answers in a row. Skipping next round");
                    player.isFrozen = true;
                    break;

                case "START":
                    if (player.isHost)
                    {
                        PromptInput("Type START to start the game.");
                    }
                    break;
                case "NEXT":
                    break;

                case "QUESTION":
                    //1 text
                    currentQuestionText = messageSplit[1];
                    //2 id
                    currentQuestionID = messageSplit[2];
                    DisplayMessage($"Next round.");
                    if (!player.isFrozen)
                    {
                        PromptInput($"{messageSplit[1]}");
                        timer.Start();
                    }
                    else
                    {
                        player.isFrozen = false;
                    }
                    break;

                case "RIGHT":
                    player.points += 2;
                    DisplayMessage("Round won, +2 points.");
                    break;

                case "WRONG":
                    player.points -= 1;
                    DisplayMessage("Wrong answer, -1 point.");
                    break;
            }
        }
    }
}
