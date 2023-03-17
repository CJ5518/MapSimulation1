using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumberOfTouches : MonoBehaviour
{
    public TextMeshProUGUI touchText = null;
    public AudioSource audioSource = null;
    public AudioClip hooray = null;
    public AudioClip que = null;
    public Slider slider = null;
    public float curr = 0f;
    bool dir = false;
    bool attempt = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount >= 5)
        {
            UpdateCurr(0.001f);

            
        }
        else
        {
            if (!audioSource.isPlaying && attempt)
                audioSource.PlayOneShot(que);
            touchText.text = "";
            attempt = false;
            curr = 0f;
            slider.value = curr;
            dir = false;
        }
    }

    public void UpdateCurr(float newVal)
    {
        attempt = true;
        if (curr > 1f) dir = true;
        if (curr < 0f && dir) 
        {
            touchText.text = "Hooray, Yay, Hooray!";
            if (!audioSource.isPlaying)
                audioSource.PlayOneShot(hooray);
            
        }
        if (curr < 0f) dir = false;
        if (!dir) curr += newVal;
        else curr -= newVal;
        slider.value = curr;
    }
}
