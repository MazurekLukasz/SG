using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    int segments;
    float xradius;
    float yradius;
    LineRenderer line;

    public void Create(int seg, float xrad,float yrad,float widthMultiplier)
    {
        segments = seg;
        xradius = xrad;
        yradius = yrad;
        line = gameObject.GetComponent<LineRenderer>();
        line.widthMultiplier = widthMultiplier;
        CreatePoints();
    }


    void CreatePoints()
    {
        line.positionCount = 0;
        line.useWorldSpace = false;
        line.positionCount = segments + 1;
        
        float x;
        float y;
        float z = -0.1f;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

            line.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / segments);
        }
    }
}