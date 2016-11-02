using UnityEngine;
using System.Collections;

public class PauzeController : MonoBehaviour {

    [SerializeField]
    private GameObject pauzeScreen;

    [SerializeField]
    private string pauzeInput = "escape";

    private LockMouse lockMouse;

    void Start() {
        lockMouse = GetComponent<LockMouse>();
        pauzeScreen.SetActive(true);
        Time.timeScale = 0;
        lockMouse.StartLockingMouse();
    }

	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(pauzeInput)) {
            SwitchPauze();
        }
	}

    private void SwitchPauze() {
        if (pauzeScreen.activeSelf)
        {
            UnPauze();
        }
        else
        {
            Pauze();
        }
    }

    private void Pauze() {
        pauzeScreen.SetActive(true);
        Time.timeScale = 0;
        lockMouse.StopLockingMouse();
    }

    private void UnPauze()
    {
        pauzeScreen.SetActive(false);
        Time.timeScale = 1;
        lockMouse.StartLockingMouse();
    }
}
