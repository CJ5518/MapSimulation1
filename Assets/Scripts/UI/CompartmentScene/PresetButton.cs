using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PresetButton : MonoBehaviour
{
    public Toggle toggle = null;
    // Start is called before the first frame update
    void Start()
    {
        if (toggle == null) toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(AskForLoad);
        if(toggle.isOn)
        {
            Invoke(nameof(AskForLoad2), 0.5f);
        }
    }

    public void AskForLoad2()
    {
        AskForLoad(true);
    }

    public void AskForLoad(bool val)
    {
        if (val == false) return;
        //Debug.Log(int.Parse(name));
        CompartmentEvents.LoadPreset(int.Parse(name));
    }
}
