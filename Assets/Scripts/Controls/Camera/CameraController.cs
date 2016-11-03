using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    [SerializeField]
    private Transform target;
    [SerializeField]
    private float distance = 5.0f;

    [SerializeField]
    private float bufferup = 1.5f;
    [SerializeField]
    private float bufferright = 0.75f;

    [SerializeField]
    private float xSpeed = 250.0f;
    [SerializeField]
    private float ySpeed = 120.0f;

    [SerializeField]
    private float yMinLimit = -20f;
    [SerializeField]
    private float yMaxLimit = 80f;

    private float x = 0.0f;
    private float y = 0.0f;

    // Use this for initialization
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (target)
        {
            distance -= .5f * Input.mouseScrollDelta.y;
            if (distance < 0)
            {
                distance = 0;
            }
            x += Input.GetAxis("Mouse X") * xSpeed;
            y -= Input.GetAxis("Mouse Y") * ySpeed;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 position = rotation * new Vector3(bufferright, 0.0f, -distance) + target.position + new Vector3(0.0f, bufferup, 0.0f);

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
