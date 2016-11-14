using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshSpawner : MonoBehaviour
{
    [SerializeField]
    private LODInfo[] detailLevels;

    [SerializeField]
    private GameObject objectToSpawn;

    [SerializeField]
    private float meshHeightMultiplier = 1;

    [SerializeField]
    private float heightOffset;

    private EndlessTerrain endlessTerrain;

    Dictionary<Vector2, MeshTerrain> meshTerrainDictionary = new Dictionary<Vector2, MeshTerrain>();

    void OnEnable()
    {
        endlessTerrain = GetComponent<EndlessTerrain>();
        detailLevels[detailLevels.Length - 1].visibleDstThreshold = endlessTerrain.MaxViewDistance;
    }

    public void SpawnMesh(MapData _mapData, AnimationCurve _heightCurve, ObstacleType[] _obstacleTypes, Vector2 _coord, int _mapChunkSize, bool _generateFromEditor = false)
    {
        Transform parentObject = transform;
        
        if (_generateFromEditor)
        {
            GetComponent<ChildsClearer>().ClearAllChilds(transform);
        }
        else
        {
            //if we are not in the editor, the parentObject is the plane from endlessTerrain
            parentObject = endlessTerrain.GetTerrainChunkTransfrom(_coord);
        }

        if (!meshTerrainDictionary.ContainsKey(_coord))
        {
            meshTerrainDictionary.Add(_coord, new MeshTerrain(_mapData.noiseMap, objectToSpawn, _heightCurve, detailLevels, _coord, parentObject, meshHeightMultiplier, heightOffset, _generateFromEditor));
        }

        Quaternion rot = new Quaternion();

        //spawn obstalces
        for (int y = 0; y < _mapChunkSize; y++)
        {
            for (int x = 0; x < _mapChunkSize; x++)
            {
                if (_mapData.obstacleData[x, y].nodeValue != 0 && _mapData.noiseMap[x, y] <= _heightCurve.keys[0].time)
                {
                    Vector3 spawnPos = parentObject.transform.position + new Vector3(x - (_mapChunkSize - 1) / 2, _obstacleTypes[_mapData.obstacleData[x, y].nodeValue].obstacle.transform.localScale.y + 0.5f, (_mapChunkSize - y) - (_mapChunkSize + 2) / 2);

                    GameObject obstacle = Instantiate(_obstacleTypes[_mapData.obstacleData[x, y].nodeValue].obstacle, spawnPos, rot) as GameObject;
                    obstacle.transform.parent = parentObject;
                }
            }
        }
    }

    void Update() {
        if (Frames.frames % Frames.framesToSkipLong == 0)
        {
            UpdateAllMeshes(endlessTerrain.ViewerPosition);
        }
    }

    private void UpdateAllMeshes(Vector2 _viewerPosition)
    {
        foreach (Vector2 key in meshTerrainDictionary.Keys) {
            meshTerrainDictionary[key].UpdateMeshTerrain(_viewerPosition);
        }
    }

    public void RemoveMeshTerrain(Vector2 _coordinate) {
        meshTerrainDictionary.Remove(_coordinate);
    }

    public class MeshTerrain
    {
        float[,] noiseMap;
        Vector2 coordinates;
        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        LODMesh collisionLODMesh;
        AnimationCurve heightCurve;

        GameObject meshObject;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        float meshHeightMultiplier;

        public float viewerDstFromNearestEdge;

        int previousLODIndex = -1;

        public MeshTerrain(float[,] _noiseMap, GameObject _objectToSpawn, AnimationCurve _heightCurve, LODInfo[] _detailLevels, Vector2 _coordinates, Transform _parentObject, float _meshHeightMultiplier, float _heightOffsets, bool _inEditorMode = false)
        {
            noiseMap = _noiseMap;
            heightCurve = _heightCurve;
            detailLevels = _detailLevels;
            meshHeightMultiplier = _meshHeightMultiplier;

            meshObject = Instantiate(_objectToSpawn, new Vector3(_parentObject.position.x, _heightOffsets, _parentObject.position.z), new Quaternion(0, 0, 0, 0)) as GameObject;//GameObject.CreatePrimitive(PrimitiveType.Plane);

            meshFilter = meshObject.GetComponent<MeshFilter>();
            meshCollider = meshObject.GetComponent<MeshCollider>();

            meshObject.transform.parent = _parentObject;

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                if (detailLevels[i].useForCollider)
                {
                    collisionLODMesh = lodMeshes[i];
                }
            }
        }

        public void UpdateMeshTerrain(Vector2 _viewerPosition) {
            viewerDstFromNearestEdge = Vector3.Distance(new Vector3(_viewerPosition.x, 0, _viewerPosition.y), meshObject.transform.position);

            int lodIndex = 0;

            //check which levelOfDetail we should have
            for (int i = 0; i < detailLevels.Length - 1; i++) {
                if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold) {
                    lodIndex = i + 1;
                } else
                {
                    break;
                }
            }

            //update the mesh if lodIndex has changed
            if (lodIndex != previousLODIndex)
            {
                LODMesh lodMesh = lodMeshes[lodIndex];
                previousLODIndex = lodIndex;
                if (lodMesh.hasMesh)
                {
                    meshFilter.mesh = lodMesh.mesh;
                }
                else
                {
                    lodMesh.GenerateLODMesh(noiseMap, meshHeightMultiplier, heightCurve);
                    meshFilter.mesh = lodMesh.mesh;
                }
            }

            //the plane the player walks on gets a mesh
            if (lodIndex == 0)
            {
                if (!collisionLODMesh.hasMesh)
                {
                    collisionLODMesh.GenerateLODMesh(noiseMap, meshHeightMultiplier, heightCurve);
                    meshCollider.sharedMesh = collisionLODMesh.mesh;
                }
                else
                {
                    meshCollider.sharedMesh = collisionLODMesh.mesh;
                }
            }
            else
            {
                meshCollider.sharedMesh = null;
            }
        }

        public void UpdateNoiseMesh(float[,] _noiseMap) {
            noiseMap = _noiseMap;
            lodMeshes[0].GenerateLODMesh(_noiseMap, meshHeightMultiplier, heightCurve);
            meshFilter.mesh = lodMeshes[0].mesh;

            collisionLODMesh.GenerateLODMesh(noiseMap, meshHeightMultiplier, heightCurve);
        }
    }


    class LODMesh
    {
        public Mesh mesh;
        public bool hasMesh;
        int lod;

        public LODMesh(int lod)
        {
            this.lod = lod;
        }

        public void GenerateLODMesh(float[,] _noiseMap, float _meshHeightMultiplier, AnimationCurve _meshHeightCurve)
        {
            mesh = MeshGenerator.GenerateTerrainMesh(_noiseMap, _meshHeightMultiplier, _meshHeightCurve, lod).GetMesh();
            hasMesh = true;
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDstThreshold;
        public bool useForCollider;
    }

    public Dictionary<Vector2, MeshTerrain> MeshTerrainDictonary {
        get { return meshTerrainDictionary; }
    }
}
