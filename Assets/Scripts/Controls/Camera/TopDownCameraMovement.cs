using UnityEngine;
using System.Collections;

public class TopDownCameraMovement : MonoBehaviour
{
    [SerializeField]
    private float cameraMoveSpeed = 10;

    [SerializeField]
    private float cameraMoveZoneWidth = 10;

    [SerializeField]
    private float cameraMoveZoneHeight = 10;

    [SerializeField]
    private bool canMoveOnStart = true;

    private float leftMoveBorder, rightMoveBorder, upMoveBorder, downMoveBorder;

    // Use this for initialization
    void Awake()
    {
        rightMoveBorder = Screen.width - cameraMoveZoneWidth;
        leftMoveBorder = cameraMoveZoneWidth;
        upMoveBorder = Screen.height - cameraMoveZoneHeight;
        downMoveBorder = cameraMoveZoneHeight;

        if (canMoveOnStart)
            StartCameraMovement();
    }

    public void StartCameraMovement() {
        StartCoroutine(CameraMovement());
    }

    public void StopCameraMovement()
    {
        StopAllCoroutines();
    }

    IEnumerator CameraMovement() {
        while(true)
        {
            if (Input.mousePosition.x < leftMoveBorder)
                transform.position -= new Vector3(cameraMoveSpeed, 0, 0);
            else if (Input.mousePosition.x > rightMoveBorder)
                transform.position += new Vector3(cameraMoveSpeed, 0, 0);
            if (Input.mousePosition.y > upMoveBorder)
                transform.position += new Vector3(0, 0, cameraMoveSpeed);
            else if (Input.mousePosition.y < downMoveBorder)
                transform.position -= new Vector3(0, 0, cameraMoveSpeed);

            yield return new WaitForFixedUpdate();
        }
    }
}