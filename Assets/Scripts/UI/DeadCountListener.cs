using UnityEngine;
using UnityEngine.UI;

public class DeadCountListener : MonoBehaviour {

    private UpdateUIText updateUIText;

    private ProgressCounter progressCounter;

    private void Start() {
        updateUIText = GetComponent<UpdateUIText>();

        if (GameObject.FindGameObjectWithTag(Tags.DontDestroyOnLoad.ToString()) != null)
        {
            progressCounter = GameObject.FindGameObjectWithTag(Tags.DontDestroyOnLoad.ToString()).GetComponent<ProgressCounter>();
            progressCounter.deadCounterAdded += HandleNewDeadCounter;
            updateUIText.UpdateText(progressCounter.DeadCounter.ToString());
        }
    }

    private void OnDisable() {
        if (GameObject.FindGameObjectWithTag(Tags.DontDestroyOnLoad.ToString()) != null)
            GameObject.FindGameObjectWithTag(Tags.DontDestroyOnLoad.ToString()).GetComponent<ProgressCounter>().deadCounterAdded -= HandleNewDeadCounter;
    }

    private void HandleNewDeadCounter(int _deadCounter) {
        updateUIText.UpdateText(_deadCounter.ToString());
    }
}
