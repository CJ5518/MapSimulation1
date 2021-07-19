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
        float[] tempdata = new float[5];

        for(int j = 0; j < 5; j++) {
            tempdata[j] = .2f;
        }
        UpdateGraph(tempdata);
    }
    #endregion
    #region Pie
    /*
     * Give graph Updated Set of Data
     */
    public void UpdateGraph(float[] data) {
        float runningTotal = 0;
        for(int i = data.Length-1; i >= 0; i--) {
            runningTotal += data[i];
            VarLabels[i].text = runningTotal + "";

            parts[i].fillAmount = runningTotal;
        }
    }
    #endregion


}
