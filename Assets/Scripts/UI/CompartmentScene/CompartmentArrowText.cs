using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompartmentArrowText : MonoBehaviour
{
    public List<CompartmentEnum.Questions> targetQuestions;
    public List<float> targetValues;
    public TMP_Text textField = null;
    public void Start()
    {
        targetValues = new List<float>();
        for(int i = 0; i < targetQuestions.Count; i++)
        {
            targetValues.Add(0f);
        }
        if (textField)
        {
            CompartmentEvents.OnQuestionValueChange += CompartmentEvents_OnQuestionValueChange;
        }
    }

    private void CompartmentEvents_OnQuestionValueChange(CompartmentEnum.Questions question, float newValue)
    {
        //Logger.Log("Button.Onquestionvaluechange");
        if (targetQuestions.Contains(question))
        {
            targetValues[targetQuestions.IndexOf(question)] = newValue;
            textField.text = MulTerms().ToString("0");
        }
    }

    private float MulTerms()
    {
        float newVal = 1f;
        foreach(float val in targetValues)
        {
            newVal *= val;
        }

        return newVal;
    }

}