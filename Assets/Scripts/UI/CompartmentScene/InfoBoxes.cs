using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class InfoBoxes : MonoBehaviour
{
    public List<CompartmentEnum.Questions> questions = null;
    //public List<string> questionsStr = null;
    // Start is called before the first frame update
    void Start()
    {
        //BackupCode
        //if(questionsStr == null)
        //{
        //    Debug.Log("Please add questions for this info box");
        //}
        //questions = new List<CompartmentQuestions.Questions>();
        //foreach(string str in questionsStr)
        //{
        //    if(Enum.TryParse(str, out CompartmentQuestions.Questions question))
        //    {
        //        questions.Add(question);
        //    }
        //    else
        //    {
        //        Debug.LogError($"{name} was unable to map {str} to a valid question");
        //    }
        //}
        CompartmentEvents.OnValueChange += UpdateBox;
        Invoke(nameof(SetupBoxes), 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupBoxes()
    {
        UpdateBox(0);
    }

    public void UpdateBox(int value)
    {
        if(questions.Contains(CompartmentEnum.QuestionIndex[value]))
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        else
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
