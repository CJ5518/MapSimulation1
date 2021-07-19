using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PieChart : MonoBehaviour {
    #region Referneces Variables
    [Header("Pie Variables")]
    [Header("First is bottom layer, last is top layer")]
    public Image[] parts;
    public TMP_Text[] VarLabels;
    #endregion
    #region Unity
    private void Start() {
    }
    #endregion
    #region Pie
    /*
     * Give graph Updated Set of Data
     */
    public void UpdateGraph(float[] data, float total) {
        float runningTotal = 0;
        for(int i = data.Length-1; i >= 0; i--) {
            runningTotal += (data[i] / total);
            VarLabels[i].text = Mathf.Round(data[i] / total * 1000000) / 10000 + "%";
            parts[i].fillAmount = runningTotal;
        }
        //This is for unity jankyness!
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)VarLabels[0].rectTransform.parent);
    }
    #endregion


}
