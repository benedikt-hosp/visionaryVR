using System.Collections.Generic;
using Content.Source.DataInformation;
using Source.DataInformation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class slider : QuestionUI
{
    private Slider _sliderInput;
    private bool _isMandatory;
    private float previousSliderValue = 0f;
    public float changeRate = 10.0f;            // the desired step

    private Text _textLeft;
    private Text _textRight;
    

    
    public override Dictionary<string, string> GetAnswersOfQuestion()
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        result.Add(_RawQuestionInformation.id,_sliderInput.value.ToString());
        return result;
        
    }
    
    public override void SetupQuestionUI(Question question, Transform parentAnchor)
    {   
        base.SetupQuestionUI(question,parentAnchor);
        _sliderInput = GetComponentInChildren<Slider>();
        _textLeft = transform.Find("Content").Find("Text_Left").GetComponent<Text>();
        _textRight = transform.Find("Content").Find("Text_Right").GetComponent<Text>();
        _textLeft.text = question.left;
        _textRight.text = question.right;
        _sliderInput.maxValue = question.tick_count;
        _sliderInput.value = (int)question.tick_count * 0.5f;

        GetComponentInChildren<Text>().text = question.instructions;
        _isMandatory = question.required;

        previousSliderValue = _sliderInput.value;

        // 

    }

    public void IncreaseValue()
    {
        if (previousSliderValue - changeRate <= _sliderInput.maxValue)
            _sliderInput.value = previousSliderValue + changeRate;
        previousSliderValue = _sliderInput.value;

    }

    public void DecreaseValue()
    {
        if(previousSliderValue - changeRate >= _sliderInput.minValue)
            _sliderInput.value = previousSliderValue - changeRate;
        previousSliderValue = _sliderInput.value;

    }


    public override bool isQuestionValid()
    {
        return true;
    }
}
