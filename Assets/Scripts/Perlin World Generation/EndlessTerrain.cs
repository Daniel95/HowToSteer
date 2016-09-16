using UnityEngine;
using System;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour {

    const float viewerMoveThresholdForChunkUpdate = 25;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    [SerializeField]
    private float maxViewDst = 225;

    [SerializeField]
    private Transform viewer;

    [SerializeField]
    private GameObject objectToSpawn;

    [SerializeField]
    private float heightOffset;

    [SerializeField]
    public int pauzeFrames = 10;

    public Action<Vector2> removeChunkDelegate;

    public Vector2 viewerPosition;
    private Vector2 viewerPositionOld;

    static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    public List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
    Dictionary<Vector2, TerrainChunk> terrainChunksToRemove = new Dictionary<Vector2, TerrainChunk>();

    void Start() {
        mapGenerator = GetComponent<MapGenerator>();

        chunkSize = mapGenerator.MapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if((viewerPositionOld-viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks() {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
            terrainChunksToRemove.Add(terrainChunksVisibleLastUpdate[i].coordinates, terrainChunksVisibleLastUpdate[i]);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (Vector2.Distance(viewedChunkCoord * chunkSize, viewerPosition) < maxViewDst)
                {
                    if (!terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, heightOffset, transform, objectToSpawn));
                        terrainChunkDictionary[viewedChunkCoord].GenerateTerrain();
                    }
                    else if (!terrainChunkDictionary[viewedChunkCoord].isVisible) {
                        terrainChunkDictionary[viewedChunkCoord].Activate();
                    }

                    terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    terrainChunksToRemove.Remove(viewedChunkCoord);
                }
            }
        }

        foreach (var key in terrainChunksToRemove.Keys)
        {
            terrainChunksToRemove[key].Deactivate(removeChunkDelegate);
        }
        terrainChunksToRemove.Clear();
    }

    public Transform GetTerrainChunkTransfrom(Vector2 _coordinates)
    {
        return terrainChunkDictionary[_coordinates].meshObject.transform;
    }

    public class TerrainChunk {
        public GameObject meshObject;
        public Vector2 coordinates;

        public bool isVisible = true;

        public TerrainChunk(Vector2 _coord, int _size, float _heightOffset, Transform _parent, GameObject _objectToSpawn)
        {
            coordinates = _coord;
            Vector2 position = _coord * _size;
            Vector3 positionV3 = new Vector3(position.x, _heightOffset, position.y);

            if (_objectToSpawn != null) {
                meshObject = Instantiate(_objectToSpawn, positionV3, new Quaternion(0,0,0,0)) as GameObject;
                meshObject.transform.localScale = Vector3.one * _size / 10f;
                meshObject.transform.parent = _parent;
            }
        }

        public void GenerateTerrain() {
            mapGenerator.RequestGenerateMap(coordinates);
        }

        public void Activate() {
            isVisible = true;
            GenerateTerrain();
            meshObject.SetActive(true);
        }

        public void Deactivate(Action<Vector2> _removeChunkDelegate) {
            isVisible = false;
            PoolChildren(_removeChunkDelegate);
            meshObject.SetActive(false);
        }

        private void PoolChildren(Action<Vector2> _removeChunkDelegate) {
            foreach (Transform child in meshObject.transform) {
                if (_removeChunkDelegate != null)
                    _removeChunkDelegate(coordinates);

                Destroy(child.gameObject);
            }
        }
    }

    public float MaxViewDistance {
        get { return maxViewDst; }
    }
}
