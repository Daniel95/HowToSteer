using UnityEngine;
using System.Collections;

public class RotateTowards : MonoBehaviour {

    [SerializeField]
    private Quaternion targetRotation;

    [SerializeField]
    private float speed = 10;

    private Quaternion startRotation;

    void Start() {
        startRotation = transform.rotation;
        targetRotation = Quaternion.Euler(targetRotation.x, targetRotation.y, targetRotation.z);

        StartCoroutine(RotateTo());
    }

    IEnumerator RotateTo() {
        float t = 0;

        while (transform.rotation != targetRotation) {
            t += speed;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            yield return new WaitForFixedUpdate();
        }
    }
}
