using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ServerLogic
{
    public class ServerManager
    {
        private static ServerManager _instance;

        public static ServerManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ServerManager();
            }

            return _instance;
        }

        private ServerManager()
        {

        }

        public TCPServerLibrary.Server server;
        List<Client> clients = new List<Client>();
        List<Answer> correctAnswers = new List<Answer>();
        int answersReceived;
        bool stateInitProcedureDone;

        enum GameState
            {
                IN_LOBBY,
                LOBBY_FILLED,
                STARTING,
                PLAYING,
                NEW_ROUND,
                ENDING
            }

        GameState gameState;

        public void Init()
        {
            gameState = GameState.IN_LOBBY;
            server = TCPServerLibrary.Server.GetInstance();
            server.Init();
        }

        public void Update()
        {
            switch (gameState)
            {
                case GameState.IN_LOBBY:
                    if (!stateInitProcedureDone)
                    {
                        server.Start();
                        server.socket.PlayerConnected += RegisterNewClient;
                        stateInitProcedureDone = true;
                    }
                    break;

                case GameState.LOBBY_FILLED:
                    if (!stateInitProcedureDone)
                    {
                        answersReceived = 0;
                        server.socket.PlayerMessageReceived += ProcessClientMessage;
                        foreach (var client in clients)
                        {
                            server.Send("NEXT", client.id);
                        }
                        stateInitProcedureDone = true;
                    }
                    break;

                case GameState.STARTING:
                    break;

                case GameState.PLAYING:
                    if (!stateInitProcedureDone)
                    {
                        SendQuestions();
                        stateInitProcedureDone = true;
                    }
                    break;

                case GameState.NEW_ROUND:

                    answersReceived = 0;
                    correctAnswers.Clear();
                    gameState = GameState.PLAYING;
                    break;

                case GameState.ENDING:
                    EndGame();
                    break;
            }
        }

        private void RegisterNewClient(int connectionID)
        {
            if (clients.Count < 4)
            {
                var newClient = ClientFactory.CreateClient(connectionID);
                newClient.role = clients.Count == 1 ? Client.Role.HOST : Client.Role.PLAYER;
                clients.Add(newClient);
            }
            else
            {
                gameState = GameState.LOBBY_FILLED;
                stateInitProcedureDone = false;
            }
        }

        /*
         * EXAMPLE START MESSAGE FOR USER WITH ID 1222
         * "1222;START"
         * 
         * EXAMPLE ANSWER MESSAGE FOR USER WITH ID 1204, QUESTION WITH CODE 20 AND TIME OF 4.25 seconds
         * "1204;ANSWER:20:YES:4.25"
         */

        private void ProcessClientMessage(int connectionID, string message)
        {
            var messageSplit = message.Split(';');
            int id = Int32.Parse(messageSplit[0]);
            Client sender = clients.Find(x => x.id == id);

            var operationSplit = messageSplit[1].Split(':');

            switch (operationSplit[0])
            {
                case "START":
                    if (sender.role == Client.Role.HOST)
                    {
                        gameState = GameState.PLAYING;
                        stateInitProcedureDone = false;
                    }
                    break;
                case "ANSWER":
                    int code = Int32.Parse(operationSplit[1]);
                    string answer = operationSplit[2];
                    float time = float.Parse(operationSplit[3], CultureInfo.InvariantCulture);
                    ProcessAnswer(new Answer() 
                    { 
                        questionCode = code,
                        answer = answer,
                        time = time,
                        senderID = id 
                    });
                    break;
            }
        }

        private void ProcessAnswer(Answer receivedAnswer)
        {

            if (receivedAnswer.time > 5.0f || !AnswerValidator.ValidateAnswer(receivedAnswer.questionCode, receivedAnswer.answer))
            {
                clients.Find(x => x.id == receivedAnswer.senderID).points -= 2;
                server.Send("WRONG", receivedAnswer.senderID);
            }
            else
            {
                correctAnswers.Add(receivedAnswer);
            }

            if (answersReceived == 4)
            {
                float minTime = 5.0f;
                Answer winningAnswer = null;

                foreach (var answer in correctAnswers)
                {
                    if (answer.time < minTime)
                    {
                        minTime = answer.time;
                        winningAnswer = answer;
                    }
                }

                clients.Find(x => x.id == winningAnswer.senderID).points += 1;
                server.Send("RIGHT", winningAnswer.senderID);
                gameState = GameState.NEW_ROUND;
                stateInitProcedureDone = false;
            }
        }

        private void SendQuestions()
        {
            var question = AnswerValidator.GetNextQuestion();

            if (question == null)
            {
                gameState = GameState.ENDING;
            }
            else
            {
                foreach (var client in clients)
                {
                    server.Send(question.questionText, client.id);
                }
            }
        }

        private void EndGame()
        {
            Client winner = null;
            int maxPoints = 0;

            foreach (var client in clients)
            {
                if (client.points > maxPoints)
                {
                    maxPoints = client.points;
                    winner = client;
                }
            }

            server.Send("WIN", winner.id);
            clients.Remove(winner);

            foreach (var client in clients)
            {
                server.Send("LOSE", client.id);
            }

            //close server
            //exit 
        }
    }
}
