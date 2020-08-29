using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour
{
    Text TurnText;

    void Start()
    {
        TurnText = GetComponent<Text>();
    }

    public void TextUpdate(float value)
    {
        TurnText.text = "" + value;
    }
}

