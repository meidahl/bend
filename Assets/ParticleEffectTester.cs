using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectTester : MonoBehaviour {

    public Vector3 speed;

	void Update () {
        transform.position = transform.position + speed * Time.deltaTime;
	}
}
