using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidImpulse : MonoBehaviour {

    public Vector3 InitialVelocity = new Vector3(0, 0, 0);

	// Use this for initialization
	void Start ()
    {
        GetComponent<Rigidbody>().velocity = InitialVelocity;
	}
}
