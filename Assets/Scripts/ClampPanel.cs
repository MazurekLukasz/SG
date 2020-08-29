using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClampPanel : MonoBehaviour
{

    [SerializeField] List<GameObject> Panels = new List<GameObject>();
    public GameObject LastSelectedObject { get; set; }
    
    // Update is called once per frame
    //void Update()
    //{
        //foreach (GameObject panel in Panels)
        //{
        //    if (panel.activeSelf && LastSelectedObject != null)
        //    {
        //        //Vector3 Pos = Camera.main.WorldToScreenPoint(LastSelectedObject.transform.position); 
        //        //panel.transform.position = Pos;
        //    }
        //}
        
    //}
}
