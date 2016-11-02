using UnityEngine;

public class WaitLevelLoaded : MonoBehaviour {

    [SerializeField]
    private bool shouldWaitForLevelLoaded = true;

    void Start() {
        if (!shouldWaitForLevelLoaded) {
            DoneLoading();
        }
        else {
            FindObjectOfType<MapGenerator>().doneLoadingLevel += DoneLoading;
        }
    }

    void OnDisable() {
        if(FindObjectOfType<MapGenerator>() != null)
            FindObjectOfType<MapGenerator>().doneLoadingLevel -= DoneLoading;
    }

    //wait until all levels have been loaded, then start setting the player down
    void DoneLoading() {
        GetComponent<SetPlayerDown>().StartSetttingDown();
    }
}
