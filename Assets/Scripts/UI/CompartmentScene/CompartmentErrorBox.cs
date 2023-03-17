using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CompartmentErrorBox : MonoBehaviour
{
    public TextMeshProUGUI errorText;
    
    public void SetText(float min, float max)
    {
        errorText.text = $"Error: This value is outside the accepted values. Please input a valid value. The eligible values are {min.ToString()} through {max.ToString()}.";
    }
}
