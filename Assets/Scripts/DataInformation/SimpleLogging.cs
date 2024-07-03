using System.Collections.Generic;
using Source.ExperimentManagement;
using UnityEngine;

namespace Source.DataInformation
{

    public class SimplifiedVector2
    {
        public float PosX { get; set; }
        public float PosY { get; set; }

        public SimplifiedVector2(float xPos, float yPos)
        {
            PosX = xPos;
            PosY = yPos;
        }
    }

    /// <summary>
    /// An example logfile
    /// </summary>
    public class SimpleLogging:ListPrintable
    {
        public string answerQuestionOne { get; set; }
        public string answerQuestionTwo { get; set; }
        public Vector2[] cursorPosition { get; set; }

        public string recordedCondition { get; set; }

        private string[] header =
        {
            "UserId",
            "TrialId",
            "Start_Condition_Group",
            "ConditionOfTrial",
            "AnswerQuestionOne",
            "AnswerQuestionTwo",
            "CursorPositions"
        };
        
        /// <summary>
        /// Make sure you return your custom header here
        /// </summary>
        /// <returns></returns>
        public override string[] GetHeaderOfFile()
        {
            return header;
        }

        /// <summary>
        /// Write all logs into a list for the Writing interface; make sure you follow the same order like in the header Array
        /// </summary>
        /// <returns></returns>
        public override List<string> GetResultsOfRecording()
        {
            List<string> result = new List<string>();
            
            result.Add(UserId);
            result.Add(TrialId.ToString());
            result.Add(ConditionId.ToString());
            result.Add(recordedCondition);
            result.Add(answerQuestionOne);
            result.Add(answerQuestionTwo);
            
            List<SimplifiedVector2> simpleVectors = new List<SimplifiedVector2>();
            foreach (var position in cursorPosition)
            {
                simpleVectors.Add(new SimplifiedVector2(position.x,position.y));
            }
            string cursorPositions = Newtonsoft.Json.JsonConvert.SerializeObject(simpleVectors);

            result.Add(cursorPositions);
            return result;
        }
    }
}