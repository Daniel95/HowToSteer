using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestructionLine : MonoBehaviour {

    [SerializeField]
    private int wayPointCooldown = 50;

    [SerializeField]
    private int headStartTime = 100;

    [SerializeField]
    private int tearWidth;

    [SerializeField]
    private float tearSmoothness = 0.5f;

    [SerializeField]
    private int skipMeshBuildingAmount = 1;
    private int amountSkippedCounter = 0;

    private int mapChunkSize;
    private int halfMapChunkSize;

    private Vector2 oldChunkPosition;
    private Vector2 oldLocalPosition;

    private MeshSpawner meshSpawner;

    private List<Vector2> chunksPassedThrough = new List<Vector2>();

    void Start()
    {
        meshSpawner = FindObjectOfType<MeshSpawner>();
        mapChunkSize = FindObjectOfType<MapGenerator>().MapChunkSize - 1;

        GetComponent<WaitForLevelLoaded>().startPlaying += StartDestructionLineScript;

        halfMapChunkSize = mapChunkSize / 2;

        oldChunkPosition = new Vector2(0,0);
        oldLocalPosition = new Vector2(halfMapChunkSize - 10, halfMapChunkSize);
    }

    void OnDisable() {
        GetComponent<WaitForLevelLoaded>().startPlaying -= StartDestructionLineScript;
    }

    void StartDestructionLineScript()
    {
        StartCoroutine(BeginningHeadStart());
    }

    IEnumerator BeginningHeadStart() {
        int counter = 0;

        while (counter < headStartTime)
        {
            counter++;
            yield return new WaitForFixedUpdate();
        }

        StartCoroutine(WaitForDestruction());
    }

    IEnumerator WaitForDestruction() {

        int counter = 0;

        Vector2 newChunkPosition = new Vector2(Mathf.Floor((transform.position.x + halfMapChunkSize) / mapChunkSize), Mathf.Floor((transform.position.z + halfMapChunkSize) / mapChunkSize));
        Vector2 newLocalPosition = new Vector2(transform.position.x - (newChunkPosition.x * mapChunkSize - 40), Mathf.Abs(transform.position.z - (newChunkPosition.y * mapChunkSize + 40)));

        while (counter < wayPointCooldown) {
            counter++;
            yield return new WaitForFixedUpdate();
        }

        Paths.CreatePathChunksOverflow(oldChunkPosition, newChunkPosition, oldLocalPosition, newLocalPosition, 1, tearWidth, tearSmoothness, true, mapChunkSize);

        int chunkCounter = 0;

        if (amountSkippedCounter >= skipMeshBuildingAmount)
        {
            amountSkippedCounter = 0;
            while (chunkCounter < chunksPassedThrough.Count)
            {
                meshSpawner.MeshTerrainDictonary[chunksPassedThrough[chunkCounter]].UpdateNoiseMesh(MapGenerator.mapDataContainer[chunksPassedThrough[chunkCounter]].noiseMap);
                chunkCounter++;
                yield return new WaitForFixedUpdate();
            }
            chunksPassedThrough.Clear();
        }
        amountSkippedCounter++;

        oldChunkPosition = newChunkPosition;
        oldLocalPosition = newLocalPosition;

        StartCoroutine(WaitForDestruction());
    }

    void SetWayPoint() {

    }

    void Update() {
        if (Frames.frames % Frames.framesToSkip == 0) {
            Vector2 currentChunkLocation = new Vector2(Mathf.Floor((transform.position.x + halfMapChunkSize) / mapChunkSize), Mathf.Floor((transform.position.z + halfMapChunkSize) / mapChunkSize));
            bool alreadySavedChunk = false;
            for (int i = 0; i < chunksPassedThrough.Count; i++)
            {
                if (currentChunkLocation == chunksPassedThrough[i]) {
                    alreadySavedChunk = true;
                }
            }

            if (!alreadySavedChunk) {
                chunksPassedThrough.Add(currentChunkLocation);
            }
        }
    }
}
