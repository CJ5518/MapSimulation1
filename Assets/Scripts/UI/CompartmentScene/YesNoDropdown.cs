using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class YesNoDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown = null;
    // Start is called before the first frame update
    void Start()
    {
        dropdown.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDestroy()
    {
        dropdown.onValueChanged.RemoveAllListeners();
    }

    public void OnValueChanged(Int32 value)
    {
        CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.PredictionHomeState, value);
    }
}
