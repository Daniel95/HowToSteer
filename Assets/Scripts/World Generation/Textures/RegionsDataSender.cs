using UnityEngine;
using System.Collections;

public class RegionsDataSender : MonoBehaviour {

    void Start() {
        Renderer renderer = GetComponent<Renderer>();
        Material material = renderer.sharedMaterial;

        material.SetFloatArray("_Heights", RegionsData.regionHeights);
        material.SetColorArray("_Colors", RegionsData.regionColors);
        material.SetColor("_ColorTest", Color.blue);
    }
}
