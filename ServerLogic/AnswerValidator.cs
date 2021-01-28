using System;
using System.Collections.Generic;
using System.Text;

namespace ServerLogic
{
    public static class AnswerValidator
    {
        // temporary solution
        private static List<Question> questions = new List<Question>()
        {
            new Question {questionText = "Is Mount Everest the highest mountain in the world?", answer = "YES", questionID = 1},
            new Question {questionText = "Is Mount Everest located in the Alps?", answer = "NO", questionID = 2},
            new Question {questionText = "Was Barrack Obama the 40th president of the United States?", answer = "NO", questionID = 3},
            new Question {questionText = "Does RGB stand for \"Red, Green, Blue\"?", answer = "YES", questionID = 4},
            new Question {questionText = "Is the sky blue?", answer = "YES", questionID = 5},
            new Question {questionText = "Are you a bot?", answer = "NO", questionID = 6},
            new Question {questionText = "Is 2 + 5 equal to 40?", answer = "NO", questionID = 7},
            new Question {questionText = "Are you human?", answer = "YES", questionID = 8},
            new Question {questionText = "Are you from the QA department?", answer = "NO", questionID = 9},
            new Question {questionText = "Did you like the quiz? (the answer is obvious)", answer = "YES", questionID = 10},

        };

        public static bool ValidateAnswer(int code, string answer)
        {
            Question question = questions.Find(x => x.questionID == code);
            return (answer == question.answer);
        }
    }
}
