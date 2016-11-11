using UnityEngine;
using System.Collections;

public class StartCameraOnLoaded : MonoBehaviour {

	// Use this for initialization
	void Start () {
        FindObjectOfType<EndlessTerrain>().doneLoadingLevel += GetComponent<TopDownCameraMovement>().StartCameraMovement;
	}

    void OnDisable() {
        if(FindObjectOfType<EndlessTerrain>() != null)
            FindObjectOfType<EndlessTerrain>().doneLoadingLevel -= GetComponent<TopDownCameraMovement>().StartCameraMovement;
    }
}
