using System.Collections.Generic;
using System.Linq;
using Content.Source.DataInformation;
using Source.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class radiogrid : QuestionUI
{

    private Transform parentAnchor;
    private Transform subScrollViewAnchor;
    
    private Dictionary<int, string> Answers = new Dictionary<int, string>();
    private List<GameObject> nodes = new List<GameObject>();
    /// <summary>
    /// Constructor replacement
    /// </summary>
    /// <param name="question"></param>
    /// <param name="parentAnchor"></param>
    public override void SetupQuestionUI(Question question, Transform parentAnchor)
    {
        base.SetupQuestionUI(question,parentAnchor);
        _RawQuestionInformation = question;
        this.parentAnchor = parentAnchor;
        subScrollViewAnchor = transform.Find("ScrollViewInternal/Viewport/Content");
        SetupLabel(question.labels);
        SetupQuestions(question.q_text.ToArray(),question.labels, question.shuffle);
    }

    /// <summary>
    /// Returns if the Questionnaire is Complete
    /// </summary>
    /// <returns></returns>
    public override bool isQuestionValid()
    {

        for (int i = 0; i < Answers.Count; i++)
        {
            if (Answers[i] == (ConstantValues.ValueNotWritten))
            {
                nodes[i].transform.Find("Text").GetComponent<Text>().color = Color.red;
            }
            else
            {
                nodes[i].transform.Find("Text").GetComponent<Text>().color = Color.black;
            }
        }


        bool isValid = !Answers.ContainsValue(ConstantValues.ValueNotWritten);
        return isValid;
    }

    /// <summary>
    /// Creates the LabelRow
    /// </summary>
    /// <param name="labels"></param>
    private void SetupLabel(string[] labels)
    {
        GameObject label = Resources.Load("UI/MatrixParts/Matrix_Header") as GameObject;
        GameObject textView = Resources.Load("UI/MatrixParts/Matrix_Label") as GameObject;
        
        GameObject newLabel = Instantiate(label, parentAnchor, true);
        newLabel.transform.SetAsFirstSibling();
        newLabel.transform.localScale = Vector3. one;
        newLabel.transform.localPosition = Vector3.zero;
        
        foreach (string text in labels)
        {
           Transform anchor = newLabel.transform.Find("Content").transform;
           GameObject  obj =  Instantiate(textView, anchor, true);
           obj.transform.localScale = Vector3.one;
           obj.transform.localPosition = Vector3.zero;
           obj.GetComponent<Text>().text = text;
          
        }
    }

    /// <summary>
    /// Creates all items of the matrix
    /// </summary>
    /// <param name="items"></param>
    /// <param name="labels"></param>
    /// <param name="needToshuffle"></param>
    private void SetupQuestions(QuestionItem[] items, string[] labels, bool needToshuffle)
    {
        GameObject label = Resources.Load("UI/MatrixParts/Matrix_Question") as GameObject;
        GameObject toggle = Resources.Load("UI/MatrixParts/Matrix_Toggle") as GameObject;

        int ID_Question = 0;


        // Create all Questions
        foreach (QuestionItem item in items)
        {
            GameObject newNodeOfMatrix = Instantiate(label, subScrollViewAnchor, true);
            //newNodeOfMatrix.transform.localPosition = Vector3.zero;
            newNodeOfMatrix.transform.localScale = Vector3.one;
            newNodeOfMatrix.transform.localPosition = Vector3.zero;
            
            newNodeOfMatrix.transform.Find("Text").GetComponent<Text>().text = item.text;
            newNodeOfMatrix.name = "Question_" + ID_Question;
            Transform anchor = newNodeOfMatrix.transform.Find("Content").transform;
            
            // Create an answer Dictonary for later
            Answers.Add(ID_Question, ConstantValues.ValueNotWritten);
            
            // Create the ToggleButtons
            int ID_Toggle = 0;

            // Add the Toogles
            foreach (string text in labels)
            {
                GameObject obj = Instantiate(toggle, anchor, true);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;
                obj.name = ID_Question  + "_" + ID_Toggle;
                
                obj.GetComponent<Toggle>().group = newNodeOfMatrix.GetComponent<ToggleGroup>();
                obj.GetComponent<Toggle>().isOn = false;
                obj.GetComponent<Toggle>().onValueChanged.AddListener(delegate(bool arg0) {  UpdateAnswerOfQuestion(obj.GetComponent<Toggle>());});
                ID_Toggle++;
            }
            nodes.Add(newNodeOfMatrix);

            ID_Question++;
            ID_Toggle = 0;
        }

        if (needToshuffle)
        {
            foreach (GameObject element in nodes)
            {
                int randomInt = UnityEngine.Random.Range(1, nodes.Count);
                element.transform.SetSiblingIndex(randomInt);
            }
        }

    }

    /// <summary>
    /// Updates the AnswerSheet of the Question
    /// </summary>
    /// <param name="selfReference"></param>
    private void UpdateAnswerOfQuestion(Toggle selfReference)
    {


        if (!selfReference.group.ActiveToggles().Any())
        {
            Debug.Log(selfReference);
            string questionId = selfReference.name.Split('_')[0];    // QUESTIONID_ToggleID
            Debug.Log(questionId);
            Answers[int.Parse(questionId)] = ConstantValues.ValueNotWritten;
            Debug.Log(Answers[int.Parse(questionId)]);
        }
        else
        {
            string[] numbers = selfReference.group.ActiveToggles().First().name.Split('_');
            Answers[int.Parse(numbers[0])] = numbers[1];
            Debug.Log("Question"+ numbers[0]+" changed to new value: " +numbers[1]);
        }

   

    }
    

    /// <summary>
    /// Returns the  with all answers of the Questions
    /// </summary>
    /// <returns></returns>
    public override Dictionary<string, string> GetAnswersOfQuestion()
    {
        Dictionary<string, string> writingAnswers = new Dictionary<string, string>();
        for (var i = 0; i < Answers.Count; i++)
        {
            writingAnswers.Add(_RawQuestionInformation.id+"_"+_RawQuestionInformation.q_text[i].id,Answers[i]);
        }

        return writingAnswers;
    }
}

