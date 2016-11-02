using UnityEngine;
using System.Collections;
using System;

public class SetPlayerDown : MonoBehaviour {

    [SerializeField]
    private bool shouldMoveDown = true;

    [SerializeField]
    private float moveDownSpeed = 0.1f;

    bool hitGround = false;

    public Action startPlaying;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (shouldMoveDown)
        {
            rb.useGravity = false;
        }
        else if (startPlaying != null)
        {
            startPlaying();
        }
    }

    public void StartSetttingDown()
    {
        if (shouldMoveDown)
            StartCoroutine(MoveDown());
        else if (startPlaying != null)
        {
            startPlaying();
        }
    }

    //move down until we hit the ground
    IEnumerator MoveDown()
    {
        while (!hitGround)
        {
            transform.Translate(new Vector3(0, -moveDownSpeed, 0));
            yield return new WaitForFixedUpdate();
        }

        rb.useGravity = true;

        if (startPlaying != null)
            startPlaying();
    }

    void OnCollisionEnter()
    {
        hitGround = true;
    }
}
