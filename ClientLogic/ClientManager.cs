using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        ManualResetEvent OutputSemaphore = new ManualResetEvent(false);

        public void Init()
        {
            client = new TCPClientLibrary.Client();
            client.Start();
            player = new Player();
            gamestate = GameState.IN_LOBBY;
            timer = new Utils.Timer();
            timer.interval = 5.0f;
            timer.TimeElapsed += SendNoAnswerMessage;
            OutputSemaphore.Set();
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

        Task inputTask;

        private void PromptInput(string promptMessage)
        {
            Console.WriteLine(promptMessage);
            var input = Console.ReadLine();
            ProcessInput(input);
        }

        private void DisplayMessage(string message)
        {
            Task.Run(() => WriteLine(message));
            OutputSemaphore.WaitOne();
        }

        private void WriteLine(string message)
        {
            Console.WriteLine(message);
            OutputSemaphore.Set();
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
                    client.Send($"ANSWER:{currentQuestionID}:YES:{timer.elapsed}<EOF>");
                    timer.raiseEvents = false;
                    break;

                case "NO":
                    client.Send($"ANSWER:{currentQuestionID}:NO:{timer.elapsed}<EOF>");
                    timer.raiseEvents = false;
                    break;
            }
        }

        private void SendNoAnswerMessage()
        {
            var sim = new WindowsInput.InputSimulator();
            sim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.END);
            client.Send($"ANSWER:{currentQuestionID}:NONE:{timer.elapsed}<EOF>");
            timer.raiseEvents = false;
        }

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
                        timer.Start();
                        inputTask = Task.Run(() => PromptInput($"{messageSplit[1]}"));
                    }
                    else
                    {
                        SendNoAnswerMessage();
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

                case "WIN":
                    DisplayMessage($"Congrats, you won! Your points:{player.points}\nPress anything to exit...");
                    Console.ReadLine();
                    Environment.Exit(0);
                    break;

                case "LOSE":
                    DisplayMessage($"You lost, better luck next time. Your points:{player.points}\nPress anything to exit...");
                    Console.ReadLine();
                    Environment.Exit(0);
                    break;
            }
        }
    }
}
