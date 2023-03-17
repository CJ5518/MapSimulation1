using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempQuestionActivation : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
