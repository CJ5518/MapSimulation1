using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderTextWholeNums : MonoBehaviour
{
    Slider slider = null;
    public TextMeshProUGUI handle = null;
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        if (handle == null)
        {
            Debug.Log("Null handle");
            return;
        }
        slider.onValueChanged.AddListener(UpdateTexts);

    }

    void UpdateTexts(float val)
    {
        handle.text = val.ToString();
    }
}
