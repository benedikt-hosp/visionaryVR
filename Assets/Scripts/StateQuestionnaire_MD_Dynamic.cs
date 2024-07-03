//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using Content.Source.DataInformation;
//using Source.ExperimentManagement;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;
////using UnityEngine.XR.Interaction.Toolkit;

//namespace Source.States
//{

//    public class StateQuestionnaire_MD_Dynamic : ExperimentState
//    {

//        private Questionnaire _questionnaire;
//        public Canvas _mainCanvas;
//        public Transform _scrollViewAnchor;
//        public Button _submitButton;
//        private string[] _questionnaireOrder;
//        private int _selectedQuestionnaire = 0;
//        public GameObject errorMsg;
//        public Canvas questionnaireCanvas;

//        // access to xtal controller
//        // set to isQuestionnaire
//        // register events from xtal controller


//        private void Awake()
//        {
//            _submitButton.onClick.AddListener(SubmitButtonPressed);
//            questionnaireCanvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();
//            questionnaireCanvas.worldCamera = Camera.main;
//        }

//        public override void StartState()
//        {
//            _questionnaireOrder = ExperimentController.GetModelOfExperiment().ReturnSceneRunningInfoOfScene().Split((','));
//            SetupNextPage();
//        }

//        private void SubmitButtonPressed()
//        {
//            if (AreAllQuestionsValid())
//            {
//                // Log All Questions
//                Dictionary<string, string> results = new Dictionary<string, string>();
//                QuestionUI[] elements = _mainCanvas.GetComponentsInChildren<QuestionUI>();
//                foreach (QuestionUI question in elements)
//                {
//                    question.GetAnswersOfQuestion().ToList().ForEach(x => results.Add(x.Key, x.Value));
//                }
//                StartCoroutine(HandleNextPage(results));
//            }
//            else
//            {
//                errorMsg.SetActive(true);
//            }

//        }

//        IEnumerator HandleNextPage(Dictionary<string, string> results)
//        {
//            yield return ExperimentController.WritingInterface.WriteQuestionnaireAsCoroutine(ExperimentStateModel.GetUserId().ToString(), results, _questionnaireOrder[_selectedQuestionnaire]);

//            _selectedQuestionnaire++;
//            if (!SetupNextPage())
//            {
//                Debug.Log("Questionnaires Done; Move to next scene");

//                ExperimentController.ChangeToNextStateOfExperiment();
//            }


//        }

//        private bool SetupNextPage()
//        {

//            Debug.Log("Next Page of the Experiment.");

//            errorMsg.SetActive(false);
//            _scrollViewAnchor.transform.localPosition = Vector3.zero;

//            if (_selectedQuestionnaire >= _questionnaireOrder.Length)
//                return false;

//            if (_scrollViewAnchor.transform.childCount > 0)
//            {
//                foreach (Transform t in _scrollViewAnchor.transform)
//                {
//                    Destroy(t.gameObject);
//                }
//            }

//            TextAsset questionnaireFile = Resources.Load("Questionnaires/" + _questionnaireOrder[_selectedQuestionnaire]) as TextAsset;
//            // Read the GameState
//            if (JSONUtilitiesGame.ConvertTextAssetToQuestionnaire(questionnaireFile, out Questionnaire questionnaire))
//            {
//                _questionnaire = questionnaire;
//                _mainCanvas.transform.Find("Title").GetComponent<Text>().text = questionnaire.title;
//                _mainCanvas.transform.Find("Description").GetComponent<Text>().text = questionnaire.instructions;

//                foreach (Question questionItem in questionnaire.questions)
//                {

//                    Debug.Log("Create Question: " + questionItem.id + " with type: " + questionItem.questiontype);
//                    GameObject newElement = Resources.Load("UI/" + questionItem.questiontype) as GameObject;
//                    GameObject obj = GameObject.Instantiate(newElement, _scrollViewAnchor, false);
//                    obj.GetComponent<QuestionUI>().SetupQuestionUI(questionItem, obj.transform);
//                    obj.transform.localPosition = new Vector3(0, obj.transform.position.y, 0);
//                    obj.transform.localScale = Vector3.one;
//                }

//                return true;
//            }

//            return false;
//        }

//        private bool AreAllQuestionsValid()
//        {
//            QuestionUI[] elements = _mainCanvas.GetComponentsInChildren<QuestionUI>();

//            foreach (QuestionUI question in elements)
//            {
//                if (!question.isQuestionValid())
//                {
//                    return false;
//                }
//            }

//            return true;
//        }

//        public void UpdateState()
//        {
//            //throw new System.NotImplementedException();
//        }

//        public override void EndState()
//        {
//            //throw new System.NotImplementedException();
//        }
//    }
//}




