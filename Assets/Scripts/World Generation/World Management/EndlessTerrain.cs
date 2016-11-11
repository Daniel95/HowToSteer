using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    [SerializeField]
    private int maxViewDst = 225;

    [SerializeField]
    private int maxDestroyOffset = 20;

    [SerializeField]
    private Transform viewer;

    [SerializeField]
    private GameObject objectToSpawn;

    [SerializeField]
    private float heightOffset;

    public Action<Vector2> chunkBeingDeactivated;
    public Action<Vector2> chunkBeingDestroyed;

    public Vector2 viewerPosition;
    private Vector2 viewerPositionOld;

    static MapGenerator mapGenerator;
    private int chunkSize;
    //the max offset a chunk could have before it is no longer possible to see it
    private int maxVisibleChunkOffset;

    Dictionary<Vector2, TerrainChunk> terrainChunkContainer = new Dictionary<Vector2, TerrainChunk>();

    public List<Vector2> coordsVisibleLastUpdate = new List<Vector2>();
    public List<Vector2> coordsToRemove = new List<Vector2>();
    //Dictionary<Vector2, TerrainChunk> terrainChunksToRemove = new Dictionary<Vector2, TerrainChunk>();

    private List<Vector2> queuedCoordsToGenerate = new List<Vector2>();

    public Action doneLoadingLevel;

    void Start()
    {
        mapGenerator = GetComponent<MapGenerator>();

        chunkSize = mapGenerator.MapChunkSize - 1;
        //the max offset a chunk could have before it is no longer possible to see it
        maxVisibleChunkOffset = Mathf.RoundToInt(maxViewDst / chunkSize);

        UpdateVisibleChunks(new Vector2(0,0));

        StartCoroutine(WaitForLevelLoaded());
    }

    void Update()
    {
        if (Frames.frames % Frames.framesToSkipLong == 0)
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

            if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
            {
                viewerPositionOld = viewerPosition;

                Vector2 currentChunkCoord = new Vector2(Mathf.RoundToInt(viewerPosition.x / chunkSize), Mathf.RoundToInt(viewerPosition.y / chunkSize));

                UpdateVisibleChunks(currentChunkCoord);
                CheckOutOfRangeChunks(currentChunkCoord);
            }
        }

        if (queuedCoordsToGenerate.Count > 0)
        {
            Generate(queuedCoordsToGenerate[0]);
            queuedCoordsToGenerate.Remove(queuedCoordsToGenerate[0]);
        }
    }

    IEnumerator WaitForLevelLoaded()
    {
        yield return new WaitForFixedUpdate();
        while (queuedCoordsToGenerate.Count > 0)
        {
            yield return new WaitForFixedUpdate();
        }

        if (doneLoadingLevel != null)
            doneLoadingLevel();
    }

    //adds or removes chunks that the player should see
    void UpdateVisibleChunks(Vector2 _currentChunkCoord)
    {
        for (int i = 0; i < coordsVisibleLastUpdate.Count; i++)
        {
            coordsToRemove.Add(coordsVisibleLastUpdate[i]);
        }
        coordsVisibleLastUpdate.Clear();

        //check all chunks within the max offset of our chunks that we would be able to see
        for (int yOffset = -maxVisibleChunkOffset; yOffset <= maxVisibleChunkOffset; yOffset++)
        {
            for (int xOffset = -maxVisibleChunkOffset; xOffset <= maxVisibleChunkOffset; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(_currentChunkCoord.x + xOffset, _currentChunkCoord.y + yOffset);

                //if the chunk is within distance
                if (Vector2.Distance(viewedChunkCoord * chunkSize, viewerPosition) < maxViewDst)
                {
                    //check if i should make a new terrainchunk, or activate an old one
                    if (!terrainChunkContainer.ContainsKey(viewedChunkCoord))
                    {
                        queuedCoordsToGenerate.Add(viewedChunkCoord);
                        terrainChunkContainer.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, heightOffset, transform, objectToSpawn));
                        //CheckOutOfRangeChunks(_currentChunkCoord);
                    }
                    else if (!terrainChunkContainer[viewedChunkCoord].isVisible)
                    {
                        terrainChunkContainer[viewedChunkCoord].Activate();
                        //CheckOutOfRangeChunks(_currentChunkCoord);
                    }

                    coordsVisibleLastUpdate.Add(viewedChunkCoord);
                    coordsToRemove.Remove(viewedChunkCoord);
                }
            }
        }

        for (int i = 0; i < coordsToRemove.Count; i++) {
            terrainChunkContainer[coordsToRemove[i]].Deactivate(chunkBeingDestroyed);
        }
        coordsToRemove.Clear();
    }

    //checks for chunks out of range and activates and removes it if it is out of range
    private void CheckOutOfRangeChunks(Vector2 _currentChunkCoord) {
        List<Vector2> coordsToRemove = new List<Vector2>();

        foreach (Vector2 coord in terrainChunkContainer.Keys)
        {
            Vector2 offset = _currentChunkCoord - coord;

            if (Mathf.Abs(offset.x) > maxDestroyOffset || Mathf.Abs(offset.y) > maxDestroyOffset)
            {
                coordsToRemove.Add(coord);
            }
        }

        for (int i = 0; i < coordsToRemove.Count; i++)
        {
            if (queuedCoordsToGenerate.Contains(coordsToRemove[i]))
            {
                terrainChunkContainer[coordsToRemove[i]].DestroyChunk(chunkBeingDestroyed, false);
                terrainChunkContainer.Remove(coordsToRemove[i]);
                queuedCoordsToGenerate.Remove(coordsToRemove[i]);
            }
            else
            {
                terrainChunkContainer[coordsToRemove[i]].DestroyChunk(chunkBeingDestroyed, true);
            }
            terrainChunkContainer.Remove(coordsToRemove[i]);
        }
    }

    public Transform GetTerrainChunkTransfrom(Vector2 _coordinates)
    {
        return terrainChunkContainer[_coordinates].meshObject.transform;
    }

    private void Generate(Vector2 _coord) {
        //FindObjectOfType<DebugGrid>().SpawnEditableMessage("Generated", _coord, "ExistingMode", 10);
        mapGenerator.GenerateMap(_coord);
    }

    public class TerrainChunk
    {
        public GameObject meshObject;
        public Vector2 coordinate;

        public bool isVisible = true;

        public TerrainChunk(Vector2 _coord, int _size, float _heightOffset, Transform _parent, GameObject _objectToSpawn)
        {
            coordinate = _coord;
            //FindObjectOfType<DebugGrid>().SpawnUniqueMessage(coordinate.ToString(), coordinate, 30, false);

            Vector2 position = _coord * _size;
            Vector3 positionV3 = new Vector3(position.x, _heightOffset, position.y);

            if (_objectToSpawn != null)
            {
                meshObject = Instantiate(_objectToSpawn, positionV3, new Quaternion(0, 0, 0, 0)) as GameObject;
                meshObject.transform.localScale = Vector3.one * _size / 10f;
                meshObject.transform.parent = _parent;
            }
        }

        public void Activate()
        {
            //FindObjectOfType<DebugGrid>().SpawnEditableMessage("Activated", coordinate, "ExistingMode", 10);
            isVisible = true;
            meshObject.SetActive(true);
        }

        public void Deactivate(Action<Vector2> _chunkBeingDeactivated)
        {
            //FindObjectOfType<DebugGrid>().SpawnEditableMessage("Deactivated", coordinate, "ExistingMode", 10);
            isVisible = false;
            meshObject.SetActive(false);
        }

        public void DestroyChunk(Action<Vector2> _chunkBeingDestroyed, bool _exists)
        {
            //FindObjectOfType<DebugGrid>().SpawnEditableMessage("Destroyed", coordinate, "ExistingMode", 10);
            if (_exists && _chunkBeingDestroyed != null)
                _chunkBeingDestroyed(coordinate);

            RemoveChildren();
            Destroy(meshObject);
        }

        private void RemoveChildren()
        {
            foreach (Transform child in meshObject.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public float MaxViewDistance
    {
        get { return maxViewDst; }
    }
}