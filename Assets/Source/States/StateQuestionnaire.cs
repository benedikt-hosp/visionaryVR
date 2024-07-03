using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Content.Source.DataInformation;
using Source.DataInformation;
using Source.ExperimentManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.XR.Interaction.Toolkit;

namespace Source.States
{
    
public class StateQuestionnaire :ExperimentState
{

        private Questionnaire _questionnaire;
        public Canvas _mainCanvas;
        public Transform _scrollViewAnchor;
        public Button _submitButton;
        private string[] _questionnaireOrder;
        private int _selectedQuestionnaire = 0;
        public GameObject errorMsg;
        public Canvas questionnaireCanvas;
        private ETController etController;
        XTAL_ControllerInput xtalC;
        private int currentActiveSlider = 0;
        public MonoBehaviour _mb;
        Dictionary<string, string> results;
        string instructionImagePath = "Images/InstructionsMap_quesstionnaire";


        Canvas instructionCanvas;
        Image instructionsImage;

        List<slider> sliderItemsLists = new List<slider>();


        private void Awake()
        {


        }

        public override void StartState()
        {

             var etc = GameObject.FindObjectsOfType<Canvas>();
            instructionCanvas = etc[0].name == "InstructionsCanvas" ? etc[0] : etc[1];
            instructionsImage = GameObject.Find("InstructionsImage").GetComponent<Image>();
            Sprite test = Resources.Load<Sprite>(instructionImagePath);
            instructionsImage.sprite = test;
            instructionsImage.enabled = true;


            _mb = GameObject.FindObjectOfType<MonoBehaviour>();
            _mainCanvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();
            _mainCanvas.worldCamera = Camera.main;

            //this.etController = new ETController(Providers.XTAL);

            this.xtalC = ExperimentController._model.xtalControllerController;
            //this.xtalc = new xtal_controllerinput();
            this.xtalC.isQuestionnaireControl = true;
            this.xtalC.PressedButtonToSubmitQuestionnaireEvent += SubmitButtonPressed;
            this.xtalC.ThumbstickPushedRightEvent += IncreaseCurrentSliderValue;
            this.xtalC.ThumbstickPushedLeftEvent += DecreaseCurrentSliderValue;
            this.xtalC.MoveToNextSliderEvent += SetNextSlider;
            this.xtalC.MoveToPreviousSliderEvent += SetPreviousSlider;

            this._questionnaireOrder =  ExperimentController._model.ReturnCommandOfRunningScene().Split((','));
            SetupNextPage();
        }

        private void SubmitButtonPressed()
        {
            if (AreAllQuestionsValid())
            {
                // Log All Questions
                results = new Dictionary<string, string>();
                QuestionUI[] elements = _mainCanvas.GetComponentsInChildren<QuestionUI>();
                foreach (QuestionUI question in elements)
                {
                    question.GetAnswersOfQuestion().ToList().ForEach(x => results.Add(x.Key, x.Value)); 
                }
                StartCoroutine(HandleNextPage(results));
            }
            else
            {
                results = null;
                errorMsg.SetActive(true);
            }
            
        }

        public void SetNextSlider()
        {
            //Debug.Log("Current Slider Nr: " + this.currentActiveSlider);

            if (this.currentActiveSlider < sliderItemsLists.Count)
            {
                this.currentActiveSlider++;
            }
            //Debug.Log("Current Slider Nr: " + this.currentActiveSlider);

        }

        public void SetPreviousSlider()
        {
            //Debug.Log("Current Slider Nr: " + this.currentActiveSlider);
            if (this.currentActiveSlider > 0)
            {
                this.currentActiveSlider--;
            }
            //Debug.Log("Current Slider Nr: " + this.currentActiveSlider);
        }


        public void IncreaseCurrentSliderValue()
        {
            //Debug.Log("IncreaseValue for Slider Nr: " + this.currentActiveSlider);
            sliderItemsLists[this.currentActiveSlider].IncreaseValue();
        }

        public void DecreaseCurrentSliderValue()
        {
           // Debug.Log("DecreaseValue for Slider Nr: " + this.currentActiveSlider);
            sliderItemsLists[this.currentActiveSlider].DecreaseValue();
        }

        IEnumerator HandleNextPage(Dictionary<string,string> results)
        {

            yield return ExperimentController.WritingInterface.WriteQuestionnaireAsCoroutine(ExperimentController._model.GetUserId().ToString(), ExperimentController._model.GetUserFolder(), ExperimentController._model.GetCurrentConditionName(), results, _questionnaireOrder[_selectedQuestionnaire]);// _selectedQuestionnaire]);

            _selectedQuestionnaire++;
                if (!SetupNextPage())
                {
                    Debug.Log("Questionnaires Done; Move to next scene");
                    ExperimentController.ChangeToNextStateOfExperiment();
                }
            


        }

        private bool SetupNextPage()
        {
            
            Debug.Log("Next Page of the Experiment.");
            
            errorMsg.SetActive(false);
            _scrollViewAnchor.transform.localPosition = Vector3.zero;

            if (_selectedQuestionnaire >= _questionnaireOrder.Length)
                return false;

            if (_scrollViewAnchor.transform.childCount > 0)
            {
                foreach (Transform t in _scrollViewAnchor.transform)
                {
                    Destroy(t.gameObject);
                }
            }

            TextAsset questionnaireFile = Resources.Load("Questionnaires/" + _questionnaireOrder[_selectedQuestionnaire]) as TextAsset;
            // Read the GameState
            if (JSONUtilitiesGame.ConvertTextAssetToQuestionnaire(questionnaireFile, out Questionnaire questionnaire))
            {
                _questionnaire = questionnaire;
                _mainCanvas.transform.Find("Title").GetComponent<Text>().text = questionnaire.title;
                _mainCanvas.transform.Find("Description").GetComponent<Text>().text = questionnaire.instructions;
                sliderItemsLists.Clear();
                currentActiveSlider = 0;


                foreach (Question questionItem in questionnaire.questions)
                {

                    //Debug.Log("Create Question: " + questionItem.id + " with type: " + questionItem.questiontype);
                    GameObject newElement = Resources.Load("UI/" + questionItem.questiontype) as GameObject;
                    GameObject obj = GameObject.Instantiate(newElement, _scrollViewAnchor, false);
                    obj.GetComponent<QuestionUI>().SetupQuestionUI(questionItem, obj.transform);
                    obj.transform.localPosition = new Vector3(0, obj.transform.position.y, 0);
                    obj.transform.localScale = Vector3.one;

                    // BHO: So far only slider are taken into account
                    sliderItemsLists.Add(obj.GetComponent<slider>());
                }

                return true;
            }

            return false;
        }




        private bool AreAllQuestionsValid()
        {
            if(_mainCanvas == null)
                _mainCanvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();

            QuestionUI[] elements = _mainCanvas.GetComponentsInChildren<QuestionUI>();

            foreach (QuestionUI question in elements)
            {
                if (!question.isQuestionValid())
                {
                    return false;
                }
            }

            return true;
        }
        
        public void UpdateState()
        {
            //throw new System.NotImplementedException();
        }

        void Update()
        {
            // LEAVE: If we want to skip the current scene
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                instructionCanvas.enabled = false;

                ExperimentController.ChangeToNextStateOfExperiment();
            }
            if (Input.GetKeyUp(KeyCode.B))
            {
                ExperimentController.ReturnToLastStateOfExperiment();
            }
        }

        public override void EndState()
        {

            instructionsImage.enabled = false;

            this.xtalC.PressedButtonToSubmitQuestionnaireEvent -= SubmitButtonPressed;
            this.xtalC.ThumbstickPushedRightEvent -= IncreaseCurrentSliderValue;
            this.xtalC.ThumbstickPushedLeftEvent -= DecreaseCurrentSliderValue;
            this.xtalC.MoveToNextSliderEvent -= SetNextSlider;
            this.xtalC.MoveToPreviousSliderEvent -= SetPreviousSlider;

            Debug.Log("EndState at Questionnaire reached and stopped all coroutines.");
        }
    }
}




