using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TimeBarGraph : MonoBehaviour
{
    #region Referneces Vars
    public List<BarSection> sections;
    public GameObject template;
    public int maxBarCount;
    #endregion
    /*
     * Update Time Graph; In raw Numbers
     */
    public void UpdateTimeBar(float[] data, float total) {
        if (sections.Count >= maxBarCount) PurgeHalf();
        //Convert to %
        for(int i = 0; i < data.Length; i++) {
            data[i] = Mathf.Round(data[i] / total * 1000000) / 1000000;
        }
        //for(int i = 1; i < sections.Count; i++) {
        //    sections[i - 1].UpdateSection(sections[i]);
        //}
        var b = Instantiate(template, this.transform).GetComponent<BarSection>();
        sections.Add(b);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        b.UpdateSection(data);
    }
    void PurgeHalf() {
        for (int i = sections.Count-2; i >= 0; i-=2) {
            var remove = sections[i].gameObject;
            sections.RemoveAt(i);
            Destroy(remove);
        }
    }
}
