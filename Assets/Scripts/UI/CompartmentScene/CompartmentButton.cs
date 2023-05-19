using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CompartmentButton : MonoBehaviour
{
    public CompartmentEnum.Questions target;
    public Button button = null;
    public TMP_InputField inputField = null;
    private GameObject errorBox = null;
    public void Start()
    {
        if (button == null) button = gameObject.GetComponent<Button>();
        if (inputField)
        {
            CompartmentEvents.OnQuestionValueChange += CompartmentEvents_OnQuestionValueChange;
        }
        //if (button == null)
        //{
        //    Logger.LogError("Do not assign this to a nonbutton");
        //    return;
        //}

    }

    private void CompartmentEvents_OnQuestionValueChange(CompartmentEnum.Questions question, float newValue)
    {
        //Logger.Log("Button.Onquestionvaluechange");
        if (question != target)
        {
            return;
        }
        if (float.TryParse(inputField.text, out float result))
        {
            if (result == newValue) return;
        }
        inputField.text = newValue.ToString(CompartmentEnum.FetchFormatting(question));
    }

    public void IncValue()
    {
        CompartmentEvents.IncValue();
    }

    public void DecValue()
    {
        CompartmentEvents.DecValue();
    }

    public void SetValue(int value)
    {
        CompartmentEvents.SetNewValue(value);
    }

    public void SetValueEnum()
    {
        //Logger.Log($"{target} {CompartmentEnum.QuestionIndex.IndexOf(target)}");
        CompartmentEvents.SetNewValue((CompartmentEnum.QuestionIndex.IndexOf(target)));
    }
    
    public void SetQuestionValue()
    {
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>();
        }
        if (float.TryParse(inputField.text, out float result))
        {
            Logger.Log($"{inputField.text} mapped to {result}");
            Logger.Log($"{target.ToString()} {((int)target).ToString()}: min {CompartmentEnum.QuestionsMinMax[(int)target].min} max {CompartmentEnum.QuestionsMinMax[(int)target].max}");
            if (CompartmentEnum.QuestionsMinMax[(int)target].min <= result && result <= CompartmentEnum.QuestionsMinMax[(int)target].max)
            {
                if (errorBox != null)
                {
                    inputField.onDeselect.RemoveListener(DestroyErrorBox);
                    Destroy(errorBox);
                }
                CompartmentEvents.SetNewQuestionValue(target, result);
            }
            else
            {
                if (errorBox != null) return;
                GameObject pfErrorBox = Resources.Load("UI/ErrorBox") as GameObject;
                errorBox = Instantiate(pfErrorBox, transform);
                errorBox.GetComponent<CompartmentErrorBox>().SetText(CompartmentEnum.QuestionsMinMax[(int)target].min, CompartmentEnum.QuestionsMinMax[(int)target].max);
                inputField.onDeselect.AddListener(DestroyErrorBox);
                inputField.onSelect.AddListener(SpawnErrorBox);
            }
        }
        else
        {
            Logger.Log("Couldn't map float");
        }

    }

    public void SpawnErrorBox(string val)
    {
        if (errorBox != null) return;
        if (float.TryParse(inputField.text, out float result))
        {
            if (CompartmentEnum.QuestionsMinMax[(int)target].min <= result && result <= CompartmentEnum.QuestionsMinMax[(int)target].max) return;
        }
        GameObject pfErrorBox = Resources.Load("UI/ErrorBox") as GameObject;
        errorBox = Instantiate(pfErrorBox, transform);
        errorBox.GetComponent<CompartmentErrorBox>().SetText(CompartmentEnum.QuestionsMinMax[(int)target].min, CompartmentEnum.QuestionsMinMax[(int)target].max);
        inputField.onDeselect.AddListener(DestroyErrorBox);
    }

    public void DestroyErrorBox(string val)
    {
        Logger.Log("Fired");
        Destroy(errorBox);
    }

}