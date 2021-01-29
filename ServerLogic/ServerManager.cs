﻿using System;
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


        public TCPServerLibrary.Server server;
        List<Client> clients = new List<Client>();
        List<Answer> correctAnswers = new List<Answer>();
        int answersReceived;
        bool stateInitProcedureDone;


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

                    if (clients.Count == 4)
                    {
                        gameState = GameState.LOBBY_FILLED;
                        stateInitProcedureDone = false;
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

                    if (answersReceived == 4)
                    {
                        DetermineRoundWinner();
                        gameState = GameState.NEW_ROUND;
                        stateInitProcedureDone = false;
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
                var newClient = ClientFactory.CreateClient(connectionID);
                newClient.role = clients.Count == 1 ? Client.Role.HOST : Client.Role.PLAYER;
                clients.Add(newClient);
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
            var client = clients.Find(x => x.id == receivedAnswer.senderID);
            if (receivedAnswer.time > 5.0f || !AnswersManager.ValidateAnswer(receivedAnswer.questionCode, receivedAnswer.answer))
            {
                client.points -= 2;
                client.wrongAnswers += 1;

                server.Send("WRONG", receivedAnswer.senderID);

                // freeze player after three wrong answers in a row
                if (client.wrongAnswers % 3 == 0 && client.wrongAnswers > 0)
                {
                    server.Send("FREEZE", receivedAnswer.senderID);
                }    
            }
            else
            {
                // clear player's wrong answer streak
                client.wrongAnswers = 0;

                correctAnswers.Add(receivedAnswer);
            }

            answersReceived++;
        }

        private void DetermineRoundWinner()
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

            var winningClient = clients.Find(x => x.id == winningAnswer.senderID);
            winningClient.points += 1;
            server.Send("RIGHT", winningClient.id);
        }

        private void SendQuestions()
        {
            var question = AnswersManager.GetNextQuestion();

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

            server.Shutdown();
        }
    }
}
