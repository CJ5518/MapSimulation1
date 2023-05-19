using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class StateDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown = null;
    List<string> alphabeticalList = new List<string>(SimulationManager.stats.stateNames);
    // Start is called before the first frame update
    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        alphabeticalList.Sort();
        for (int i = 0; i < SimulationManager.stats.stateNames.Count; i++)
        {
            dropdown.options.Add((new TMP_Dropdown.OptionData(alphabeticalList[i])));
            //Logger.Log(SimulationStats.stateNames[i]);
        }
        dropdown.onValueChanged.AddListener(OnValueChanged);
        //dropdown.options.AddRange(SimulationStats.stateNames);
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
