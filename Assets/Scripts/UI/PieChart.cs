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
     * Give graph Updated Set of Data; In raw Numbers
     */
    public void UpdateGraph(float[] data, float total) {
        float runningTotal = 0;
        for(int i = data.Length-1; i >= 0; i--) {
            runningTotal += (data[i] / total);

			
            VarLabels[i].text = data[i] / total > .999f? "100 " : (data[i] * 100 / total).ToString("##0.00");
			if (VarLabels[i].text.Length > 4) {
				VarLabels[i].text = VarLabels[i].text.Substring(0, 4);
			}
			VarLabels[i].text += "%";
			
			if (data[i].ToString("F0").Length > 3)
				VarLabels[i].text += " " + data[i].ToString("F0").Substring(0, 3);
			else
				VarLabels[i].text += " " + data[i].ToString("F0");

			VarLabels[i].text += data[i] > 1000f ? (data[i] > 1000000f ? "m" : "k") : ("");

			parts[i].fillAmount = runningTotal;
        }
        //This is for unity jankyness!
        //Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)VarLabels[0].rectTransform.parent);
    }
    #endregion
}