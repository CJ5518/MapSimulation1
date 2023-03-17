using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionManager : MonoBehaviour
{
    //public List<InfoBoxes> questions = new List<InfoBoxes>();
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(transform.childCount);

        //CompartmentEvents.OnValueChange += SetValue;
        //transform.GetChild(0).gameObject.SetActive(true);
        SetupChildren();
        //Invoke(nameof(SetupChildren), 0.1f);
        CompartmentEvents.LoadPreset(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupChildren()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            //transform.GetChild(i).gameObject.SetActive(false);
            //Debug.Log($"Adding {transform.GetChild(i).GetComponent<CompartmentQuestion>().question} to list");
            //Each question self enrolls at start
            CompartmentEnum.QuestionIndex.Add(transform.GetChild(i).GetComponent<CompartmentQuestion>().question);
        }
    }

    public void SetValue(int newValue)
    {
        //Cyclical calling by ListenerWrapper
        if (newValue == CompartmentEvents.oldVal)
        {
            Debug.LogError("QuestionManager.SetValue new value same as old");
            return;
        }
        transform.GetChild(CompartmentEvents.oldVal - 1).gameObject.SetActive(false);
        transform.GetChild(newValue - 1).gameObject.SetActive(true);
    }
}
