using System.Collections;
using System.Collections.Generic;
using Source.DataInformation;
using Source.ExperimentManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Source.ExperimentStates
{
    /// <summary>
    /// This is just an example task
    /// </summary>
    public class StateExampleTask:ExperimentState
    {
        
        public Text roundId;
        public Text conditionText;
        public InputField answerQuestionOne;
        public InputField answerQuestionTwo;
        public Button submitButton;
        private bool _isTrialRunning = false;

        public int trialRepetitions = 3;
        private List<SimpleLogging> logsOfTrial;
        private SimpleLogging currentLog;
        public override void StartState()
        {
            logsOfTrial = new List<SimpleLogging>();
            
            conditionText.text = ExperimentController._model.ReturnCommandOfRunningScene();        // BHO: how to get current Condition from EXController
            
            answerQuestionOne.onValueChanged.AddListener(AnswerQuestionOne);
            answerQuestionTwo.onValueChanged.AddListener(AnswerQuestionTwo);
            submitButton.onClick.AddListener(SubmitButtonPressed);
            
            
            StartCoroutine(PerformTask());
        }

        private void SubmitButtonPressed()
        {
            _isTrialRunning = false;
        }

        /// <summary>
        /// use this is you want to run multiple trials in one task
        /// </summary>
        /// <returns></returns>
        IEnumerator PerformTask()
        {
            // We want to have X repetitions of the same task
            for (int trialId = 0; trialId < trialRepetitions; trialId++)
            {
                yield return PerformSingleTrial(trialId);
            }
            
            yield return ExperimentController.WritingInterface.WriteTrialsIntoCsvFile(logsOfTrial.ToArray(),"%_NoServer");
            // We're done with this condition
            ExperimentController.ChangeToNextStateOfExperiment();
            
            
        }

        /// <summary>
        /// Performs one trial from the beginning to the end including data recording
        /// </summary>
        /// <returns></returns>
        IEnumerator PerformSingleTrial(int trialId)
        {
            // Reset Logging
            List<Vector2> cursorPosition = new List<Vector2>();
            currentLog = SetupLoggingOfTrial(trialId);

            
            // Setup Trial
            answerQuestionOne.text = "";
            answerQuestionTwo.text = "";
            submitButton.interactable = false;
            roundId.text = trialId.ToString();
            
            _isTrialRunning = true;
            while (_isTrialRunning)
            {
                cursorPosition.Add(Input.mousePosition);
                yield return null;
            }
            
            // Store the Cursor data in the log file
            currentLog.cursorPosition = cursorPosition.ToArray();
            
            logsOfTrial.Add(currentLog);
            
        }

        private SimpleLogging SetupLoggingOfTrial( int trialId)
        {
            SimpleLogging logData = new SimpleLogging();
            logData = new SimpleLogging();
            logData.UserId = ExperimentStateModel.GetUserId();
            logData.ConditionId = (int) ExperimentStateModel.GetProtocolID();
            logData.TrialId = trialId;
            logData.recordedCondition = ExperimentStateModel.ReturnCommandOfRunningScene();

            return logData;
        }

        private void AnswerQuestionTwo(string arg0)
        {
            currentLog.answerQuestionTwo = arg0;
            submitButton.interactable = answerQuestionOne.text.Length > 3 && answerQuestionTwo.text.Length>3;
        }

        private void AnswerQuestionOne(string arg0)
        {
            currentLog.answerQuestionOne = arg0;
            submitButton.interactable = answerQuestionOne.text.Length > 3 && answerQuestionTwo.text.Length>3;
        }
        
        /// <summary>
        /// You can add the data logging here or in the end of the Loop of the IEnumerator
        /// </summary>
        public override void EndState()
        {

        }
    }
}