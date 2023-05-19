using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompartmentToggle : MonoBehaviour
{
    public Toggle toggle = null;
    public CompartmentEnum.Questions targetQuestion;
    // Start is called before the first frame update
    void Start()
    {
        if (toggle == null) toggle = GetComponent<Toggle>();
        if(targetQuestion == CompartmentEnum.Questions.None)
        {
            Logger.Log($"{transform.parent.parent.parent.name} {transform.parent.name} {name} Please assign a target question in the editor");
        }
        toggle.onValueChanged.AddListener(PrintName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PrintName(bool val)
    {
        if (val == false) return;
        print($"{targetQuestion} {name} {val}");
        CompartmentEvents.SetNewConfidence(targetQuestion, int.Parse(name));
    }
}
