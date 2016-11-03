using UnityEngine;
using System.Collections;

public class PlayerTerrainTrail : MonoBehaviour {

    [SerializeField]
    private int headStartTime = 100;

    [SerializeField]
    private bool shouldDestroy = true;

    private TerrainTrail terrainTrail; 

    void Awake() {
        GetComponent<SetPlayerDown>().startPlaying += StartDestructionLineScript;
        terrainTrail = GetComponent<TerrainTrail>();
        terrainTrail.GetPostion += ReturnPlayerPos;
    }

    void OnDisable()
    {
        GetComponent<SetPlayerDown>().startPlaying -= StartDestructionLineScript;
        terrainTrail.GetPostion -= ReturnPlayerPos;
    }

    void StartDestructionLineScript()
    {
        if (shouldDestroy)
        {
            StartCoroutine(BeginningHeadStart());
        }
    }

    IEnumerator BeginningHeadStart()
    {
        int counter = 0;

        while (counter < headStartTime)
        {
            counter++;
            yield return new WaitForFixedUpdate();
        }

        terrainTrail.StartTerrainTrails();
    }

    private Vector3 ReturnPlayerPos()
    {
        return transform.position;
    }
}
