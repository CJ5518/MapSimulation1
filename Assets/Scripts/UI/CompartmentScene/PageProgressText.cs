using UnityEngine;
using TMPro;

public class PageProgressText : MonoBehaviour {

    TextMeshProUGUI counterTextBottom = null;

    void Start() {
        counterTextBottom = gameObject.GetComponent<TextMeshProUGUI>();
        CompartmentEvents.OnValueChange += UpdateTexts;

    }
    public void UpdateTexts(int value) {
        value++;
        counterTextBottom.text = value.ToString("0") + "/" + CompartmentEvents.maxVal;
    }
}