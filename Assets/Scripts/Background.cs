using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public float Parralax = 2f;

    // Update is called once per frame
    void Update()
    {
        //MeshRenderer MR = GetComponent<MeshRenderer>();
        //Material mat = MR.material;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Material mat = sr.material;
        Vector2 offset = mat.mainTextureOffset;

        offset.x = transform.position.x / transform.localScale.x / Parralax;
        offset.y = transform.position.y / transform.localScale.y / Parralax;

        mat.mainTextureOffset = offset;
    }
}
