using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour {

    private enum DrawMode { NoiseMap, ColourMap, VoxelMap, MeshMap };
    [SerializeField]
    private DrawMode drawMode;

    [SerializeField]
    private Noise.NormalizeMode normalizeMode;

    [SerializeField]
    private AnimationCurve heightCurve;

    [SerializeField]
    private int mapChunkSize = 40;

    [SerializeField]
    public float noiseScale = 25;

    [SerializeField]
    private int octaves = 5;

    [Range(0,1)]
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

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();

    Dictionary<Vector2, MapData> mapDataContainer = new Dictionary<Vector2, MapData>();

    private LevelGenerator levelGenerator;

    void Start() {
        levelGenerator = GetComponent<LevelGenerator>();
        seed = UnityEngine.Random.Range(-100000, 100000);
    }

    public void GenerateMapInEditor() {
        levelGenerator = GetComponent<LevelGenerator>();
        MapData mapData = GenerateMapData(offset);

        mapData = levelGenerator.GenerateLevelObstacles(mapData, mapChunkSize, heightCurve.keys[0].time);

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
            Color[] colourMap = GetComponent<ColourMapConverter>().GenerateColorMap(mapData.noiseMap, mapChunkSize);

            GetComponent<MeshSpawner>().SpawnMesh(mapData, TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize), heightCurve, levelGenerator.obstacleTypes, mapData.coordinates, mapChunkSize, true);
        }
        //CaveMap
        else if (drawMode == DrawMode.VoxelMap)
        {
            GetComponent<CubesSpawner>().SpawnCubes(mapData, levelGenerator.obstacleTypes, heightCurve.keys[0].time, mapData.coordinates, mapChunkSize, true);
        }
    }

    public void GenerateMapInPlayMode(MapData _mapData) {
        _mapData = levelGenerator.GenerateLevelObstacles(_mapData, mapChunkSize, heightCurve.keys[0].time);

        //tweak the first chunk to have a circle in the middle so the player has room to start
        if (_mapData.coordinates == Vector2.zero)
        {
            _mapData = levelGenerator.FlattenCircleRandomized(_mapData, 0, true, LevelGenerator.FigureMode.Circle, new Vector2(mapChunkSize / 2, mapChunkSize / 2), mapChunkSize / 2, mapChunkSize, 10);
        }

        //NoiseMap
        if (drawMode == DrawMode.NoiseMap)
        {
            GetComponent<TextureDisplay>().DrawTexture(TextureGenerator.TextureFromHeightMap(_mapData.noiseMap));
        }
        //ColourMap
        else if (drawMode == DrawMode.ColourMap)
        {
            Color[] colourMap = GetComponent<ColourMapConverter>().GenerateColorMap(_mapData.noiseMap, mapChunkSize);

            GetComponent<TextureDisplay>().DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.MeshMap) {
            Color[] colourMap = GetComponent<ColourMapConverter>().GenerateColorMap(_mapData.noiseMap, mapChunkSize);

            GetComponent<MeshSpawner>().SpawnMesh(_mapData, TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize), heightCurve, levelGenerator.obstacleTypes, _mapData.coordinates, mapChunkSize, false);
        }
        //VoxelMap
        else if (drawMode == DrawMode.VoxelMap)
        {
            GetComponent<CubesSpawner>().SpawnCubes(_mapData, levelGenerator.obstacleTypes, heightCurve.keys[0].time, _mapData.coordinates, mapChunkSize, false);
        }
    }

    public void RequestGenerateMap(Vector2 _position)
    {
        ThreadStart threadStart = delegate {
            MapDataThread(GenerateMapInPlayMode, _position);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Action<MapData> _callBack, Vector2 _position)
    {
        MapData mapdata = GenerateMapData(_position);

        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(_callBack, mapdata));
        }
    }

    void Update() {
        if (mapDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callBack(threadInfo.parameter);
            }
        }
    }

    private MapData GenerateMapData(Vector2 _offset) {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, _offset * (mapChunkSize - 1), normalizeMode);

        return new MapData(noiseMap, _offset, mapChunkSize, LevelGenerator.LevelMode.Swamp);
    }

    void OnValidate() {
        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octaves < 0) {
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
}

[System.Serializable]
public struct TerrianType {
    public string name;
    public float height;
    public Color colour;
}

public struct MapData {
    public float[,] noiseMap;
    public ObstacleData[,] obstacleData;
    public Vector2 coordinates;
    public LevelGenerator.LevelMode levelMode;

    public MapData(float[,] _noiseMap, Vector2 _coordinates, int _mapChunkSize, LevelGenerator.LevelMode _levelMode) {
        this.noiseMap = _noiseMap;
        this.coordinates = _coordinates;
        this.obstacleData = new ObstacleData[_mapChunkSize, _mapChunkSize];
        this.levelMode = _levelMode;
    }
}