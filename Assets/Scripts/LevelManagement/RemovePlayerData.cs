using UnityEngine;
using System.Collections;

public class RemovePlayerData : MonoBehaviour {

    // Use this for initialization
    public void RemovePlayerProgressData()
    {
        if (GameObject.FindGameObjectWithTag(Tags.DontDestroyOnLoad.ToString()) != null)
        {
            Destroy(GameObject.FindGameObjectWithTag(Tags.DontDestroyOnLoad.ToString()));
        }
    }
}
