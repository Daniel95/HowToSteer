using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class MapGenerator : MonoBehaviour
{
    private enum DrawMode { NoiseMap, ColourMap, VoxelMap, MeshMap };
    [SerializeField]
    private DrawMode drawMode;

    [SerializeField]
    private NoiseGenerator.NormalizeMode normalizeMode;

    [SerializeField]
    private AnimationCurve heightCurve;

    [SerializeField]
    private int mapChunkSize = 40;

    [SerializeField]
    public float noiseScale = 25;

    [SerializeField]
    private int octaves = 5;

    [Range(0, 1)]
    [SerializeField]
    private float persistance = 0.5f;
    [SerializeField]
    private float lacunarity;
    [SerializeField]
    public int seed;

    [SerializeField]
    public Vector2 offset;
    [SerializeField]
    public bool autoUpdate = true;

    public static Dictionary<Vector2, MapData> mapDataContainer = new Dictionary<Vector2, MapData>();

    private MeshSpawner meshSpawner;

    private EnvironmentGenerator environmentGenerator;
    private LevelGenerator levelGenerator;
    private EndlessTerrain endlessTerrain;

    private NeighbourKnowledgeQue neighbourKnowledgeQue;

    void Start()
    {
        mapDataContainer.Clear();

        levelGenerator = GetComponent<LevelGenerator>();
        environmentGenerator = GetComponent<EnvironmentGenerator>();
        meshSpawner = GetComponent<MeshSpawner>();

        neighbourKnowledgeQue = GetComponent<NeighbourKnowledgeQue>();
        seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
    }

    void OnEnable()
    {
        endlessTerrain = GetComponent<EndlessTerrain>();
        endlessTerrain.chunkBeingDestroyed += DestroyFarAwayChunk;
    }

    void OnDisable()
    {
        if (endlessTerrain.chunkBeingDestroyed != null)
            endlessTerrain.chunkBeingDestroyed -= DestroyFarAwayChunk;
    }

    private void DestroyFarAwayChunk(Vector2 _coordinate)
    {
        //if this coord doesnt exists in the mapDataContainer, it was destroyed before leaving the QueuedCoordsToGenerate queue
        neighbourKnowledgeQue.RemoveNeighbourKnowledge(_coordinate);
        levelGenerator.RemoveIslandData(_coordinate);
        meshSpawner.RemoveMeshTerrain(_coordinate);
        mapDataContainer.Remove(_coordinate);
    }

    //check if we need to rebuild an existing mapdata, or a new one
    public void GenerateMap(Vector2 _coord)
    {
        GenerateBiome( RetrieveMapData(_coord));
    }

    //first phase of generating
    //generates a new mapdata, find its biome and add it to the mapdata container. 
    //after that we enter the StartNeighboursExistQueue 
    private void GenerateBiome(MapData _mapData)
    {
        _mapData.levelMode = environmentGenerator.GetBiomeMode(_mapData.coordinate);
        _mapData = neighbourKnowledgeQue.GetAllNeighbours(_mapData);
        mapDataContainer.Add(_mapData.coordinate, _mapData);

        GenerateEnviroment(_mapData);
    }

    //second phase of generating, generate the environment
    private void GenerateEnviroment(MapData _mapData)
    {
        _mapData = environmentGenerator.GenerateEnvironment(_mapData, mapChunkSize);
        _mapData.generatedEnviromentComplete = true;

        mapDataContainer[_mapData.coordinate] = _mapData;
        //FindObjectOfType<DebugGrid>().SpawnEditableMessage("[___]", _mapData.coordinate, "GeneratePhase");
        neighbourKnowledgeQue.StartNeighboursEnviromentQueue(_mapData.coordinate, _mapData.allNeighboursCoords, GenerateLevel);
    }

    //third phase of generating, edits the noise to create a level
    private void GenerateLevel(MapData _mapData)
    {
        _mapData = levelGenerator.GenerateLevel(_mapData, mapChunkSize);
        _mapData.generatedLevelComplete = true;

        mapDataContainer[_mapData.coordinate] = _mapData;
        //FindObjectOfType<DebugGrid>().SpawnEditableMessage("__________", _mapData.coordinate, "GeneratePhase");
        neighbourKnowledgeQue.StartNeighboursLevelQueue(_mapData.coordinate, _mapData.allNeighboursCoords, CreateMap);
    }

    //generates the map how it is given, without editing it.
    private void CreateMap(MapData _mapData)
    {
        meshSpawner.SpawnMesh(_mapData, heightCurve, levelGenerator.obstacleTypes, _mapData.coordinate, mapChunkSize);
    }

    //returns a new or existing map
    private MapData RetrieveMapData(Vector2 _offset)
    {
        MapData mapData;

        if (mapDataContainer.ContainsKey(_offset))
            mapData = mapDataContainer[_offset];
        else
            mapData = GetMapData(_offset);

        return mapData;
    }

    //generates noise depening on the offset
    private MapData GetMapData(Vector2 _offset)
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoise(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, _offset * (mapChunkSize - 1), normalizeMode);

        return new MapData(noiseMap, _offset, mapChunkSize);
    }

    void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }

    public int MapChunkSize
    {
        get { return mapChunkSize; }
    }

    public Dictionary<Vector2, MapData> MapDataContainer
    {
        get { return mapDataContainer; }
    }
}

public class MapData
{
    public float[,] noiseMap;
    public ObstacleData[,] obstacleData;
    public Vector2 coordinate;
    public EnumTypes.BiomeMode levelMode;
    public List<Vector2> allNeighboursCoords;
    public List<Vector2> directNeighbourCoords;
    public List<Vector2> cornerNeighbourCoords;

    public bool generatedEnviromentComplete;
    public bool generatedLevelComplete;

    public MapData(float[,] _noiseMap, Vector2 _coordinate, int _mapChunkSize)
    {
        noiseMap = _noiseMap;
        coordinate = _coordinate;
        obstacleData = new ObstacleData[_mapChunkSize, _mapChunkSize];
        levelMode = EnumTypes.BiomeMode.Land;
        allNeighboursCoords = new List<Vector2>();
        directNeighbourCoords = new List<Vector2>();
        cornerNeighbourCoords = new List<Vector2>();
        generatedEnviromentComplete = false;
        generatedLevelComplete = false;
    }
}