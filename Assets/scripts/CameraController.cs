using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public Transform target;
    public Vector3 offset;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void LateUpdate () {

        float x = target.forward.x * offset.z;
        float y = offset.y;
        float z = target.forward.z * offset.z;
        Vector3 camOffset = new Vector3(x, y, z);

        // TODO: Use Vector3.SmoothDamp for a smoothly camera
        transform.position = target.position + camOffset;
        transform.forward = target.forward;

    }

}
