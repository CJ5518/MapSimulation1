using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SliderHandleText : MonoBehaviour
{
    TextMeshProUGUI sliderHandleText = null;
    // Start is called before the first frame update
    void Start()
    {
        sliderHandleText = gameObject.GetComponent<TextMeshProUGUI>();
        CompartmentEvents.OnValueChange += UpdateTexts;
    }

    public void UpdateTexts(int value)
    {

        sliderHandleText.text = (value + 1).ToString("0");
    }
    
}
