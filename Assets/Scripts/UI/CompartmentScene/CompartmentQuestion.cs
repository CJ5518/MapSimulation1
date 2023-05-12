using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompartmentQuestion : MonoBehaviour
{
    public CompartmentEnum.Questions question = CompartmentEnum.Questions.None;
    // Start is called before the first frame update
    void Start()
    {
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
        //Debug.Log("Hello");
        //Debug.Log($"{value} {CompartmentEnum.QuestionIndex[value]}");
        if (question == (CompartmentEnum.QuestionIndex[value]))
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
