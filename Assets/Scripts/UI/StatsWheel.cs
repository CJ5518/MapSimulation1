using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
public class StatsWheel : MonoBehaviour
{
    public UILineRenderer linerend;
    public float[] angles;
    public Slider[] sliders;
    private void Start() {
        //draw all defaults
        for(int i = 0; i < sliders.Length; i++) {
            DrawPolygon(i);
        }
    }
    public void DrawPolygon(int slider) {

        //How long should this leg be
        float length = sliders[slider].value / sliders[slider].maxValue * 100;

        //Vector of leg
        Vector2 leg = new Vector2(0, length);

        //rotate leg
        leg = Quaternion.AngleAxis(angles[slider], Vector3.forward) * leg;

        //Set LineRend to use that new leg; If start also set end point to match
        linerend.Points[slider] = leg;
        if (slider == 0) linerend.Points[linerend.Points.Length-1] = leg;

        linerend.SetAllDirty();
    }
}
