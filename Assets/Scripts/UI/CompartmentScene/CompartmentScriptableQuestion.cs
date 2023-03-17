using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CompartmentQuestion", menuName = "CompartmentQuestion")]
public class CompartmentScriptableQuestion : ScriptableObject
{
    [Header("Admin")]
    public CompartmentEnum.Questions question = CompartmentEnum.Questions.None;


    [Header("Question Bounds")]
    public float minVal = 0f;
    public float maxVal = 0f;

}
