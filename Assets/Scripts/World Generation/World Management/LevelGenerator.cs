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
    private int minSpecialAmount = 0;

    [SerializeField]
    private int maxSpecialAmount = 5;

    [SerializeField]
    private int specialGroundClearingSize = 20;

    private Islands islands;

    void Start()
    {
        islands = GetComponent<Islands>();
    }

    //edits the mapdata by adding obstacles, and then returns the new version
    public MapData GenerateLevel(MapData _mapData, int _mapChunkSize)
    {
        if (_mapData.levelMode == EnumTypes.BiomeMode.Land)
        {
            if (_mapData.coordinate != new Vector2(0, 0))
                _mapData = GenerateSpecials(_mapData, _mapChunkSize, minSpecialSpawnHeight);
        }
        else if (_mapData.levelMode == EnumTypes.BiomeMode.Water)
        {
            _mapData = islands.GenerateIslands(_mapData, _mapChunkSize, minSpecialSpawnHeight);
        }

        return _mapData;
    }

    private MapData GenerateSpecials (MapData _mapData, int _mapChunkSize, float _heigtCurveStartValue)
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

    public void RemoveIslandData(Vector2 _coord) {
        islands.RemoveIslandData(_coord);
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