using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BarSection : MonoBehaviour
{
    #region References Vars
    public RectTransform rectTrans;
    public RectTransform[] parts;
    #endregion

    /*
     * Given Percentages Update heights
     */
    public void UpdateSection(float[] data) {
        float height = rectTrans.sizeDelta.y;
        float totalHeight = 0;
        for(int i = data.Length-1; i > 0; i--) {
            totalHeight += height * data[i];
            parts[i].sizeDelta = new Vector2(parts[i].sizeDelta.x, totalHeight);
        }
    }
    /*
     * Given Previous bar copy heights
     */
    public void UpdateSection(BarSection bar) {
        for(int i = 0; i < parts.Length; i++) {
            parts[i].sizeDelta = bar.parts[i].sizeDelta;
        }
    }
}
