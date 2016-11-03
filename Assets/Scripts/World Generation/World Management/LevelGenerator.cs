using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    public ObstacleType[] obstacleTypes;

    [SerializeField]
    private float minSpecialSpawnHeight = 0.55f;

    [SerializeField]
    private int spawnLocationOffset;

    [SerializeField]
    private int minSpecialAmount = 0;

    [SerializeField]
    private int maxSpecialAmount = 5;

    [SerializeField]
    private int specialGroundClearingSize = 20;

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
            mapTypeSeed = Random.Range(int.MinValue, int.MaxValue);
            while (GetLevelMode(new Vector2(0,0)) != EnumTypes.BiomeMode.Land)
            {
                mapTypeSeed = Random.Range(int.MinValue, int.MaxValue);
            }
        }
        else
        {
            mapTypeSeed = 0;
        }
    }

    public EnumTypes.BiomeMode GetLevelMode(Vector2 _coords) {
        int currLevelMode = Mathf.FloorToInt(Mathf.PerlinNoise((_coords.x + mapTypeSeed) / 10, (_coords.y + mapTypeSeed) / 10) * levelModes.Length);

        return levelModes[currLevelMode];
    }

    //edits the mapdata by adding obstacles, and then returns the new version
    public MapData GenerateLevel(MapData _mapData, int _mapChunkSize)
    {
        if (_mapData.levelMode == EnumTypes.BiomeMode.Land)
        {
            _mapData = ManageChunkGradient(_mapData, smoothSizeDivider, _mapChunkSize);
            if (_mapData.coordinates != new Vector2(0, 0))
                _mapData = GenerateSwamp(_mapData, _mapChunkSize, minSpecialSpawnHeight);
        }
        else if (_mapData.levelMode == EnumTypes.BiomeMode.Water)
        {
            _mapData = islands.GenerateIslands(_mapData, _mapChunkSize, minSpecialSpawnHeight);
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
                    _mapData = SmoothToDirectNeighbours
                    (
                        _mapData, 
                        _mapData.directNeighbourCoords[i] - _mapData.coordinates, 
                        _sizeDivider, 
                        _mapChunkSize
                    );
                }
            }
            /*
            for (int i = 0; i < _mapData.cornerNeighbourCoords.Count; i++)
            {
                if (MapGenerator.mapDataContainer[_mapData.directNeighbourCoords[i]].levelMode == EnumTypes.BiomeMode.Water)
                {
                    _mapData = SmoothToCornerNeighbours
                    (
                        _mapData,
                        _mapData.directNeighbourCoords[i] - _mapData.coordinates,
                        _sizeDivider,
                        _mapChunkSize
                    );
                }
            }*/
        }

        return _mapData;
    }

    public MapData SmoothToDirectNeighbours(MapData _mapData, Vector2 _dir, int _sizeDivider, int _mapChunkSize)
    {
        if (_dir.x != 0) {
            _mapData.noiseMap = NoiseEditor.SmoothSideDirX(_mapData.noiseMap, (int)_dir.x, _sizeDivider, _mapChunkSize);
        }
        if (_dir.y != 0)
        {
            _mapData.noiseMap = NoiseEditor.SmoothSideDirY(_mapData.noiseMap, (int)_dir.y, _sizeDivider, _mapChunkSize);
        }

        return _mapData;
    }
    
    public MapData SmoothToCornerNeighbours(MapData _mapData, Vector2 _dir, int _sizeDivider, int _mapChunkSize)
    {
        if (_dir.x != 0)
        {
            _mapData.noiseMap = NoiseEditor.SmoothCornerDirX(_mapData.noiseMap, (int)_dir.x, _sizeDivider, _mapChunkSize);
        }
        if (_dir.y != 0)
        {
            _mapData.noiseMap = NoiseEditor.SmoothCornerDirY(_mapData.noiseMap, (int)_dir.y, _sizeDivider, _mapChunkSize);
        }

        return _mapData;
    }

    private MapData GenerateSwamp (MapData _mapData, int _mapChunkSize, float _heigtCurveStartValue)
    {
        ObstacleData[,] obstacleData = new ObstacleData[_mapChunkSize, _mapChunkSize];

        //spawn a random amount of island at random locations in the chunk.
        int randomSpecialCount = Random.Range(minSpecialAmount, maxSpecialAmount);
        for (int i = 0; i < randomSpecialCount; i++)
        {
            Vector2 spawnPos = new Vector2(Random.Range(specialGroundClearingSize, _mapChunkSize - specialGroundClearingSize), Random.Range(specialGroundClearingSize, _mapChunkSize - specialGroundClearingSize));

            //if the spawnPos we generated is not high enough, generate a new one until we have a good one
            while (_mapData.noiseMap[(int)spawnPos.x, (int)spawnPos.y] >= _heigtCurveStartValue) {
                spawnPos = new Vector2(Random.Range(specialGroundClearingSize, _mapChunkSize - specialGroundClearingSize), Random.Range(specialGroundClearingSize, _mapChunkSize - specialGroundClearingSize));
            }

            _mapData = NoiseEditor.FlattenNoiseArea(_mapData, 0, true, EnumTypes.FigureMode.Circle, spawnPos, specialGroundClearingSize, _mapChunkSize);
            obstacleData[(int)spawnPos.x, (int)spawnPos.y].nodeValue = Mathf.FloorToInt((obstacleTypes.Length - 1) * Random.Range(0, 0.99f)) + 1;
        }

        _mapData.obstacleData = obstacleData;
        obstacleData = null;
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