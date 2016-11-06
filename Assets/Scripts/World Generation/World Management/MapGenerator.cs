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

    public Action doneLoadingLevel;

    private List<Vector2> QueuedMapdataPositionsToGenerate = new List<Vector2>();

    public static Dictionary<Vector2, MapData> mapDataContainer = new Dictionary<Vector2, MapData>();

    private MeshSpawner meshSpawner;

    private EnvironmentGenerator environmentGenerator;
    private LevelGenerator levelGenerator;

    private NeighbourKnowledgeQue neighbourKnowledgeQue;

    void Start()
    {
        mapDataContainer.Clear();

        meshSpawner = GetComponent<MeshSpawner>();
        environmentGenerator = GetComponent<EnvironmentGenerator>();
        levelGenerator = GetComponent<LevelGenerator>();
        neighbourKnowledgeQue = GetComponent<NeighbourKnowledgeQue>();
        seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        StartCoroutine(WaitForLevelLoaded());
    }

    public void GenerateMapInEditor()
    {
        levelGenerator = GetComponent<LevelGenerator>();
        MapData mapData = GetMapData(offset);

        mapData = levelGenerator.GenerateLevel(mapData, mapChunkSize);

        //NoiseMap
        if (drawMode == DrawMode.NoiseMap)
        {
            GetComponent<TextureDisplay>().DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.noiseMap));
        }
        //ColourMap
        else if (drawMode == DrawMode.ColourMap)
        {
            Color[] colourMap = GetComponent<ColourMapConverter>().GenerateColorMap(mapData.noiseMap, mapChunkSize);

            GetComponent<TextureDisplay>().DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.MeshMap)
        {
            GetComponent<MeshSpawner>().SpawnMesh(mapData, heightCurve, levelGenerator.obstacleTypes, mapData.coordinates, mapChunkSize, true);
        }
        //CaveMap
        else if (drawMode == DrawMode.VoxelMap)
        {
            GetComponent<CubesSpawner>().SpawnCubes(mapData, levelGenerator.obstacleTypes, heightCurve.keys[0].time, mapData.coordinates, mapChunkSize, true);
        }
    }

    //check if we need to rebuild an existing mapdata, or a new one
    private void CheckHowToBuild(MapData _mapData)
    {
        //generate the mapdata how it existed
        if (mapDataContainer.ContainsKey(_mapData.coordinates))
        {
            GenerateMap(_mapData);
        }
        else // generate a new mapdata
        {
            GenerateMapdata(_mapData);
        }
    }

    //first phase of generating
    //generates a new mapdata, and adds it to the mapdata container. 
    //after that we enter the StartNeighboursExistQueue 
    private void GenerateMapdata(MapData _mapData)
    {
        _mapData.levelMode = environmentGenerator.GetBiomeMode(_mapData.coordinates);
        _mapData = neighbourKnowledgeQue.GetAllNeighbours(_mapData);
        mapDataContainer.Add(_mapData.coordinates, _mapData);

        neighbourKnowledgeQue.StartNeighboursExistQueue(_mapData.coordinates, _mapData.allNeighboursCoords, GenerateEnvironment);
    }

    //second phase of generating, generate the enviroment
    private void GenerateEnvironment(MapData _mapData)
    {
        _mapData = environmentGenerator.GenerateEnvironment(_mapData, mapChunkSize);
        _mapData.generatedEnviromentComplete = true;

        mapDataContainer[_mapData.coordinates] = _mapData;

        neighbourKnowledgeQue.StartNeighboursEnviromentQueue(_mapData.coordinates, _mapData.allNeighboursCoords, GenerateLevel);
    }

    //third phase of generating, edits the noise to create a level
    private void GenerateLevel(MapData _mapData)
    {
        _mapData = levelGenerator.GenerateLevel(_mapData, mapChunkSize);
        _mapData.generatedLevelComplete = true;

        mapDataContainer[_mapData.coordinates] = _mapData;

        neighbourKnowledgeQue.StartNeighboursLevelQueue(_mapData.coordinates, _mapData.allNeighboursCoords, GenerateMap);
    }

    //generates the map how it is given, without editing it.
    private void GenerateMap(MapData _mapData)
    {
        meshSpawner.SpawnMesh(_mapData, heightCurve, levelGenerator.obstacleTypes, _mapData.coordinates, mapChunkSize);
    }


    public void RequestGenerateMap(Vector2 _position)
    {
        QueuedMapdataPositionsToGenerate.Add(_position);
    }

    //check the first mapdata in the mapdata and generate it, then remove it out of the dict, continue next frame
    void Update()
    {
        if (QueuedMapdataPositionsToGenerate.Count > 0 && Frames.frames % Frames.one == 0)
        {
            CheckHowToBuild(RetrieveMapData(QueuedMapdataPositionsToGenerate[0]));
            QueuedMapdataPositionsToGenerate.Remove(QueuedMapdataPositionsToGenerate[0]);
        }
    }

    IEnumerator WaitForLevelLoaded()
    {

        yield return new WaitForFixedUpdate();
        while (QueuedMapdataPositionsToGenerate.Count > 0)
        {
            yield return new WaitForFixedUpdate();
        }

        if (doneLoadingLevel != null)
            doneLoadingLevel();
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

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callBack;
        public readonly T parameter;

        public MapThreadInfo(Action<T> _callBack, T _parameter)
        {
            this.callBack = _callBack;
            this.parameter = _parameter;
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
    public Vector2 coordinates;
    public EnumTypes.BiomeMode levelMode;
    public List<Vector2> allNeighboursCoords;
    public List<Vector2> directNeighbourCoords;
    public List<Vector2> cornerNeighbourCoords;
    public List<Vector2> cantOverwrite;

    public bool generatedEnviromentComplete;
    public bool generatedLevelComplete;

    public MapData(float[,] _noiseMap, Vector2 _coordinates, int _mapChunkSize)
    {
        noiseMap = _noiseMap;
        coordinates = _coordinates;
        obstacleData = new ObstacleData[_mapChunkSize, _mapChunkSize];
        levelMode = EnumTypes.BiomeMode.Land;
        allNeighboursCoords = new List<Vector2>();
        directNeighbourCoords = new List<Vector2>();
        cornerNeighbourCoords = new List<Vector2>();
        generatedEnviromentComplete = false;
        generatedLevelComplete = false;
        cantOverwrite = new List<Vector2>();
    }
}