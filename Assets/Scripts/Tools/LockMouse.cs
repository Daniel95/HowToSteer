using UnityEngine;
using System.Collections;

public class LockMouse : MonoBehaviour {

    public void StartLockingMouse()
    {
        StopAllCoroutines();
        StartCoroutine(LockingMouse());
    }

    public void StopLockingMouse()
    {
        StopAllCoroutines();
        Cursor.lockState = CursorLockMode.None;
    }

    IEnumerator LockingMouse()
    {
        while (true)
        {
            Cursor.lockState = CursorLockMode.Locked;
            yield return new WaitForEndOfFrame();
        }
    }
}
