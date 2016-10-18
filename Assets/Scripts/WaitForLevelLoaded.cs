using UnityEngine;
using System.Collections;
using System;

public class WaitForLevelLoaded : MonoBehaviour {

    [SerializeField]
    private float moveDownSpeed = 0.1f;

    public Action startPlaying;

    bool gameIsStarted = false;

    Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(MoveDown());
    }

    IEnumerator MoveDown() {
        rb.useGravity = false;

        while (!gameIsStarted) {
            transform.Translate(new Vector3(0,-moveDownSpeed, 0));
            yield return new WaitForFixedUpdate();
        }

        rb.useGravity = true;

        if (startPlaying != null)
            startPlaying();
    }

    void OnCollisionEnter() {
        gameIsStarted = true;
    }
}
