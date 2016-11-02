using UnityEngine;
using System.Collections;

public class ResetControlSettings : MonoBehaviour {

    private void Start()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.None;
    }
}
