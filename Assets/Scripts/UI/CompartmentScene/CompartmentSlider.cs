using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompartmentSlider : MonoBehaviour
{
    public Slider slider = null;
    public CompartmentEnum.Questions targetQuestion;
    // Start is called before the first frame update
    void Start()
    {
        if(slider == null)
        {
            slider = GetComponent<Slider>();
        }
        if(targetQuestion == CompartmentEnum.Questions.None)
        {
            Debug.LogError("Please assign a question to this slider");
        }
        slider.minValue = CompartmentEnum.QuestionsMinMax[(int)targetQuestion].min;
        slider.maxValue = CompartmentEnum.QuestionsMinMax[(int)targetQuestion].max;
        if(CompartmentEnum.FetchFormatting(targetQuestion) == "0")
        {
            slider.wholeNumbers = true;
        }
        slider.onValueChanged.AddListener(SetQuestionValue);
        CompartmentEvents.OnQuestionValueChange += SetSliderValue;
    }

    public void SetQuestionValue(float val)
    {
        CompartmentEvents.SetNewQuestionValue(targetQuestion, val);
    }

    /// <summary>
    /// Syncs slider value to value typed into text box or preloaded into sim
    /// </summary>
    /// <param name="question"></param>
    /// <param name="value"></param>
    public void SetSliderValue(CompartmentEnum.Questions question, float value)
    {        
        if(question != targetQuestion)
        {
            return;
        }
        if (value == slider.value) return;
        slider.value = value;
    }
}
