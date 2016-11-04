using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActivateButtonKeyInput : MonoBehaviour {

    [SerializeField]
    private string buttonInput = "backspace";

    void OnEnable() {
        StartCoroutine(AwaitKeyInput());
    }

    void OnDisable() {
        StopAllCoroutines();
    }

    IEnumerator AwaitKeyInput() {
        while (!Input.GetKeyDown(buttonInput)) {
            yield return null;
        }

        GetComponent<Button>().onClick.Invoke();
    }
}
