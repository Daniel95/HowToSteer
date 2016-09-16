using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {

    [SerializeField]
    private float rotateSpeed = 0.5f;

	// Update is called once per frame
	void FixedUpdate () {
        transform.Rotate(new Vector3(0, rotateSpeed, 0));
	}

    public float RotateSpeed {
        set { rotateSpeed = value; }
    }
}
