using UnityEngine;
using System.Collections;

public class LifeTime : MonoBehaviour {

    private int timeToLive = 500;
	
	// Update is called once per frame
	void FixedUpdate () {
        timeToLive--;
        if (timeToLive < 0)
            Destroy(gameObject);
    }

    public int TimeToLive {
        set { timeToLive = value; }
    }
}
