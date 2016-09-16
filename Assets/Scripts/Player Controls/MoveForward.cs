using UnityEngine;
using System.Collections;

public class MoveForward : MonoBehaviour {

    [SerializeField]
    private float maxMovementSpeed = 8;

    [SerializeField]
    private float movementIncreaseMultiplier = 1.02f;

    private float moveSpeed = 1;

    private Rigidbody rb;
    private ConstantForce cf;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }

	// Update is called once per frame
	void FixedUpdate () {
        if (moveSpeed < maxMovementSpeed)
            moveSpeed *= movementIncreaseMultiplier;


        //add our own constant force, without removing the gravity of our rigidbodys
        rb.velocity = transform.forward * moveSpeed + new Vector3(0, rb.velocity.y, 0);
    }
}
