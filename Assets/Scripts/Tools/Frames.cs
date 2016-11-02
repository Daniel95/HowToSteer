using UnityEngine;
using System.Collections;

public class Frames : MonoBehaviour {

    public static int frames;

    public static int framesToSkipLong = 20;

    public static int framesToSkipMedium = 10;

    public static int framesToSkipFast = 3;

    public static int one = 1;

    // Update is called once per frame
    void FixedUpdate () {
        frames++;
    }
}
