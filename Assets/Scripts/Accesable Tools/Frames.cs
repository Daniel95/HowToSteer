using UnityEngine;
using System.Collections;

public class Frames : MonoBehaviour {

    public static int frames;

    public static int framesToSkip = 20;

	// Update is called once per frame
	void FixedUpdate () {
        frames++;
    }
}
