using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    public ObstacleType[] obstacleTypes;

    [SerializeField]
    private int spawnLocationOffset;

    [SerializeField]
    private float chanceToSpawnSpecial = 0.003f;

    [SerializeField]
    private int smoothSizeDivider = 10;

    [SerializeField]
    private EnumTypes.BiomeMode[] levelModes;

    [SerializeField]
    private bool randomStartPos = true;

    private Islands islands;

    private int mapTypeSeed;

    void Start()
    {
        islands = GetComponent<Islands>();

        if (randomStartPos)
        {
            mapTypeSeed = Random.Range(0, 10);
            while (GetLevelMode(new Vector2(0,0)) != EnumTypes.BiomeMode.Land)
            {
                mapTypeSeed = Random.Range(0, 10);
            }
        }
        else
        {
            mapTypeSeed = 0;
        }
    }

    public EnumTypes.BiomeMode GetLevelMode(Vector2 _coords) {
        int currLevelMode = Mathf.FloorToInt(Mathf.PerlinNoise((_coords.x + mapTypeSeed) / 10, (_coords.y + mapTypeSeed) / 10) * levelModes.Length);

        if (currLevelMode == -1) {

            print(Mathf.PerlinNoise((_coords.x + mapTypeSeed) / 10, (_coords.y + mapTypeSeed) / 10) * levelModes.Length);
        }

        return levelModes[currLevelMode];
    }

    //edits the mapdata by adding obstacles, and then returns the new version
    public MapData GenerateLevel(MapData _mapData, int _mapChunkSize, float _heigtCurveStartValue)
    {

        if (_mapData.levelMode == EnumTypes.BiomeMode.Land)
        {
            _mapData = ManageChunkGradient(_mapData, smoothSizeDivider, _mapChunkSize);
            if (_mapData.coordinates != new Vector2(0, 0))
                _mapData = GenerateSwamp(_mapData, _mapChunkSize, _heigtCurveStartValue);
        }
        else if (_mapData.levelMode == EnumTypes.BiomeMode.Water)
        {
            _mapData = islands.GenerateIslands(_mapData, _mapChunkSize, _heigtCurveStartValue);
        }

        return _mapData;
    }

    public MapData ManageChunkGradient(MapData _mapData, int _sizeDivider, int _mapChunkSize) {

        //the swamps adjusts itself to islands
        if (_mapData.levelMode == EnumTypes.BiomeMode.Land)
        {
            for (int i = 0; i < _mapData.directNeighbourCoords.Count; i++)
            {
                if (MapGenerator.mapDataContainer[_mapData.directNeighbourCoords[i]].levelMode == EnumTypes.BiomeMode.Water)
                {
                    _mapData = AdjustDirectNeighboursToIslands
                    (
                        _mapData, 
                        _mapData.directNeighbourCoords[i] - _mapData.coordinates, 
                        _sizeDivider, 
                        _mapChunkSize
                    );
                }
            }
        }

        return _mapData;
    }

    public MapData AdjustDirectNeighboursToIslands(MapData _mapData, Vector2 _dir, int _sizeDivider, int _mapChunkSize)
    {
        if (_dir.x != 0) {
            _mapData.noiseMap = NoiseEditor.RemoveSmoothDirX(_mapData.noiseMap, (int)_dir.x, _sizeDivider, _mapChunkSize);
        }
        if (_dir.y != 0)
        {
            _mapData.noiseMap = NoiseEditor.RemoveSmoothDirY(_mapData.noiseMap, (int)_dir.y, _sizeDivider, _mapChunkSize);
        }

        return _mapData;
    }

    private MapData GenerateSwamp (MapData _mapData, int _mapChunkSize, float _heigtCurveStartValue)
    {
        ObstacleData[,] obstacleData = new ObstacleData[_mapChunkSize, _mapChunkSize];

        //edits the noise, and marks obstacles if needed
        for (int y = 0; y < _mapChunkSize; y++)
        {
            for (int x = 0; x < _mapChunkSize; x++)
            {
                if (!_mapData.obstacleData[x, y].cannotOverwrite)
                {
                    if (_mapData.noiseMap[x, y] <= _heigtCurveStartValue && Random.Range(0, 0.99f) < chanceToSpawnSpecial && !NoiseEditor.CheckOverlap(new Vector2(x, y), 15, _mapChunkSize))
                    {
                        _mapData = NoiseEditor.FlattenNoiseArea(_mapData, 0, true, EnumTypes.FigureMode.Circle, new Vector2(x, y), 25, _mapChunkSize);
                        obstacleData[x, y].nodeValue = Mathf.FloorToInt((obstacleTypes.Length - 1) * Random.Range(0, 0.99f)) + 1;
                    }
                }
            }
        }

        _mapData.obstacleData = obstacleData;

        return _mapData;
    }
}

[System.Serializable]
public struct ObstacleType
{
    public string name;
    public int value;
    public GameObject obstacle;
    public bool cannotOverwrite;
}

public struct ObstacleData
{
    public int nodeValue;
    public bool cannotOverwrite;
    public bool isBorder;
    public Vector3 direction;

    public Vector2 xNeighbours;
    public Vector2 yNeighbours;

    public ObstacleData(int _nodeValue, bool _cannotOverwrite, bool _isBorder, Vector3 _direction)
    {
        this.nodeValue = _nodeValue;
        this.cannotOverwrite = _cannotOverwrite;
        this.isBorder = _isBorder;
        this.direction = _direction;
        this.xNeighbours = Vector2.zero;
        this.yNeighbours = Vector2.zero;
    }
}