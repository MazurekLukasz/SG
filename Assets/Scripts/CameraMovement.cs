using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    [SerializeField] private float yMin;
    [SerializeField] private float yMax;
    [SerializeField] private float xMin;
    [SerializeField] private float xMax;

    [SerializeField] private Transform tf;
    [SerializeField] private float speed;

    void Start ()
    {
     
	}

	void LateUpdate ()
    {
        //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Movement();
        //ChangePos(tf);
	}
    void Movement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        transform.position = new Vector3(transform.position.x + (horizontal * speed), transform.position.y + vertical * speed, -11f);
    }

    public void ChangePos(Transform target)
    {
        //transform.position = new Vector3(Mathf.Clamp(target.position.x, xMin, xMax), Mathf.Clamp(target.position.y, yMin, yMax), transform.position.z);
        transform.position = target.position;
    }

}
