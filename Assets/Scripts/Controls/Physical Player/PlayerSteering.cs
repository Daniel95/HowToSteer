using UnityEngine;

public class PlayerSteering : MonoBehaviour
{
    [SerializeField]
    private Transform cameraView;

    [SerializeField]
    private float maxRotateSpeed = 170;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, cameraView.rotation, maxRotateSpeed * Time.deltaTime);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }
}
