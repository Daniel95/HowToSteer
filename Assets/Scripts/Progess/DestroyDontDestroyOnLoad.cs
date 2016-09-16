using UnityEngine;
using System.Collections;

public class DestroyDontDestroyOnLoad : MonoBehaviour {

    void Awake() {
        if (GameObject.FindGameObjectWithTag(Tags.DontDestroyOnLoad.ToString()) != null)
            Destroy(GameObject.FindGameObjectWithTag(Tags.DontDestroyOnLoad.ToString()));
    }
}
