using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Header tab manager to manage active and unactive header tabs
/// </summary>
public class CompartmentHeaderTabs : MonoBehaviour
{
    public List<CompartmentEnum.Questions> presetBuckets = null;
    public List<CompartmentEnum.Questions> parameterBuckets = null;
    public List<CompartmentEnum.Questions> layerBuckets = null;
    public List<CompartmentEnum.Questions> predictionBuckets = null;
    public List<GameObject> activeTabs = null;
    public List<GameObject> unactiveTabs = null;
    

    public enum TabNames
    {
        Presets,
        Parameters,
        Layers,
        Predictions
    }

    private int oldVal = 0;

    private void Start()
    {
        CompartmentEvents.OnValueChange += UpdateTabs;
    }

    /// <summary>
    /// Takes current question value and sets correct header tab as active
    /// </summary>
    /// <param name="value"></param>
    private void UpdateTabs(int value)
    {
        
        value = (int)CompartmentEnum.QuestionIndex[value];
        
        if (presetBuckets.Contains((CompartmentEnum.Questions)(value)))
        {
            if (oldVal == (int)TabNames.Presets) return;
            activeTabs[oldVal].SetActive(false);
            unactiveTabs[oldVal].SetActive(true);
            activeTabs[(int)TabNames.Presets].SetActive(true);
            unactiveTabs[(int)TabNames.Presets].SetActive(false);
            oldVal = (int)TabNames.Presets;
        }
        else if(parameterBuckets.Contains((CompartmentEnum.Questions)(value)))
        {
            if (oldVal == (int)TabNames.Parameters) return;
            activeTabs[oldVal].SetActive(false);
            unactiveTabs[oldVal].SetActive(true);
            activeTabs[(int)TabNames.Parameters].SetActive(true);
            unactiveTabs[(int)TabNames.Parameters].SetActive(false);
            oldVal = (int)TabNames.Parameters;
        }
        else if(layerBuckets.Contains((CompartmentEnum.Questions)(value)))
        {
            if (oldVal == (int)TabNames.Layers) return;
            activeTabs[oldVal].SetActive(false);
            unactiveTabs[oldVal].SetActive(true);
            activeTabs[(int)TabNames.Layers].SetActive(true);
            unactiveTabs[(int)TabNames.Layers].SetActive(false);
            oldVal = (int)TabNames.Layers;
        }
        else if(predictionBuckets.Contains((CompartmentEnum.Questions)(value)))
        {
            if (oldVal == (int)TabNames.Predictions) return;
            activeTabs[oldVal].SetActive(false);
            unactiveTabs[oldVal].SetActive(true);
            activeTabs[(int)TabNames.Predictions].SetActive(true);
            unactiveTabs[(int)TabNames.Predictions].SetActive(false);
            oldVal = (int)TabNames.Predictions;
        }
        else
        {
            Debug.LogError("HeaderTabs Error: value outside of range");
        }

    }
}
