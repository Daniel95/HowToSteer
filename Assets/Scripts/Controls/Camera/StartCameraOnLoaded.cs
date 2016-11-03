using UnityEngine;
using System.Collections;

public class StartCameraOnLoaded : MonoBehaviour {

	// Use this for initialization
	void Start () {
        FindObjectOfType<MapGenerator>().doneLoadingLevel += GetComponent<TopDownCameraMovement>().StartCameraMovement;
	}

    void OnDisable() {
        if(FindObjectOfType<MapGenerator>() != null)
            FindObjectOfType<MapGenerator>().doneLoadingLevel -= GetComponent<TopDownCameraMovement>().StartCameraMovement;
    }
}
