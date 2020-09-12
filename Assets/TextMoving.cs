using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextMoving : MonoBehaviour
{
    Text txt;
    string dot = ".";
    public string text = "Bot Turn";
    // Start is called before the first frame update
    void Start()
    {
        txt = gameObject.GetComponent<Text>(); 
    }

    private void OnEnable()
    {
        StartCoroutine(Move());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator Move()
    {
        while (true)
        {
            if (dot != "...")
            {
                dot += ".";
            }
            else
            {
                dot = "";
            }
            txt.text = text + dot;
            yield return new WaitForSeconds(.5f);
        }
    }
}
