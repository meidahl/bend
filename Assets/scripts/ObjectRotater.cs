using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotater : MonoBehaviour {

    public float turnSpeed;
    public Vector3 rotateAround;
	
	void Update () {
        transform.RotateAround(rotateAround, Vector3.up, turnSpeed * Time.deltaTime);
	}
}
