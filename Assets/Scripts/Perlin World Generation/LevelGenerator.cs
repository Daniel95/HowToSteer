using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    public ObstacleType[] obstacleTypes;

    [SerializeField]
    private float chanceToSpawnSpecial = 0.003f;

    public enum FigureMode { Square, Circle };

    public enum LevelMode { Swamp, Islands };

    [SerializeField]
    private LevelMode levelMode;

    //edits the mapdata by adding obstacles, and then returns the new version
    public MapData GenerateLevelObstacles(MapData _mapData, int _mapChunkSize, float _heigtCurveStartValue)
    {
        if (_mapData.levelMode == LevelMode.Swamp)
        {
            _mapData = GenerateSwamp(_mapData, _mapChunkSize, _heigtCurveStartValue);
        }
        else if (_mapData.levelMode == LevelMode.Islands)
        {
            _mapData = GenerateIslands(_mapData, _mapChunkSize, _heigtCurveStartValue);
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
                    if (_mapData.noiseMap[x, y] <= _heigtCurveStartValue && UnityEngine.Random.Range(0, 0.99f) < chanceToSpawnSpecial && !CheckOverlap(new Vector2(x, y), 15, _mapChunkSize))
                    {
                        _mapData = FlattenNoiseArea(_mapData, 0, true, FigureMode.Circle, new Vector2(x, y), 25, _mapChunkSize);
                        obstacleData[x, y].nodeValue = 3;
                    }
                }
            }
        }

        _mapData.obstacleData = obstacleData;

        return _mapData;
    }

    private MapData GenerateIslands (MapData _mapData, int _mapChunkSize, float _heigtCurveStartValue)
    {
        ObstacleData[,] obstacleData = new ObstacleData[_mapChunkSize, _mapChunkSize];

        float[,] oldNoiseMap = new float[_mapChunkSize, _mapChunkSize];
        System.Array.Copy(_mapData.noiseMap, oldNoiseMap, _mapData.noiseMap.GetLength(0) * _mapData.noiseMap.GetLength(1));

        //edits the noise to be 
        for (int y = 0; y < _mapChunkSize; y++)
        {
            for (int x = 0; x < _mapChunkSize; x++)
            {
                _mapData.noiseMap[x, y] = 1f;
            }
        }

        //edits the noise, and marks obstacles if needed
        for (int y = 0; y < _mapChunkSize; y++)
        {
            for (int x = 0; x < _mapChunkSize; x++)
            {
                if (!_mapData.obstacleData[x, y].cannotOverwrite)
                {
                    int randomSize = Random.Range(15, 35);

                    if (oldNoiseMap[x, y] <= 0.5f && !CheckOverlap(new Vector2(x, y), randomSize, _mapChunkSize))
                    {
                        if (Random.Range(0, 0.99f) < 0.040)
                        {
                            _mapData = FlattenCircleRandomized(_mapData, 10, true, FigureMode.Circle, new Vector2(x, y), randomSize, _mapChunkSize, Random.Range(10,20));
                            //obstacleData[x, y].nodeValue = 3;
                        }
                    }
                }
            }
        }

        _mapData.obstacleData = obstacleData;

        return _mapData;
    }

    //edits the noise to dissapear smooth on the X as
    private MapData RemoveSmoothDirX(MapData _mapData, int _dirX, int _mapChunkSize) {

        for (int y = 0; y < _mapChunkSize; y ++)
        {
            for (int xCounter = 0; xCounter < _mapChunkSize; xCounter++)
            {
                int x = xCounter;

                //if the dir is inverted, invert the x in the grid (on a symmetric pos)
                if (_dirX > 0)
                    x = _mapChunkSize - xCounter;

                _mapData.noiseMap[xCounter, y] /= (float)x / (float)_mapChunkSize;
            }
        }

        return _mapData;
    }

    //edits the noise to dissapear smooth on the Y as
    private MapData RemoveSmoothDirY(MapData _mapData, int _dirY, int _mapChunkSize)
    {
        for (int yCounter = 0; yCounter < _mapChunkSize; yCounter++)
        {
            for (int x = 0; x < _mapChunkSize; x++)
            {
                int y = yCounter;

                //if the dir is inverted, invert the x in the grid (on a symmetric pos)
                if (_dirY > 0)
                    y = _mapChunkSize - yCounter;

                _mapData.noiseMap[x, y] /= (float)y / (float)_mapChunkSize;
            }
        }

        return _mapData;
    }

    //removes all noise from the map to it is flat.
    private MapData MapBasisRemove(MapData _mapData, int _mapChunkSize)
    {
        for (int y = 0; y < _mapChunkSize; y++)
        {
            for (int x = 0; x < _mapChunkSize; x++)
            {
                _mapData.noiseMap[x, y] = 1f;
            }
        }

        return _mapData;
    }

    public bool CheckOverlap(Vector2 _clearPosition, int _clearSize, int _mapChunkSize)
    {
        if (_clearPosition.x > _clearSize && _clearPosition.x < _mapChunkSize - _clearSize && _clearPosition.y > _clearSize && _clearPosition.y < _mapChunkSize - _clearSize)
            return false;
        else
            return true;
    }

    public MapData FlattenCircleRandomized(MapData _mapData, int _nodeValue,  bool _cannotOverwrite, FigureMode _figureMode, Vector2 _clearPosition, int _clearSize, int _mapChunkSize, int _randomizeAmount) {

        FlattenNoiseArea(_mapData, _nodeValue, _cannotOverwrite, FigureMode.Circle, _clearPosition, _clearSize, _mapChunkSize);

        for (int i = 0; i < _randomizeAmount; i++)
        {
            Vector2 randomCirclePosition = new Vector2(_clearPosition.x + (_clearSize / 2.5f) * (Random.Range(-0.5f, 0.5f) * 2), _clearPosition.y + (_clearSize / 2.5f) * (Random.Range(-0.5f, 0.5f) * 2));

            int randomClearSize = Mathf.RoundToInt(_clearSize / Random.Range(1f, 2f));

            if (CheckOverlap(randomCirclePosition, randomClearSize, _mapChunkSize))
                _mapData = FlattenNoiseArea(_mapData, _nodeValue, _cannotOverwrite, FigureMode.Circle, randomCirclePosition, randomClearSize, _mapChunkSize);
        }

        return _mapData;
    }

    //replaces an given area
    public MapData FlattenNoiseArea(MapData _mapData, int _nodeValue, bool _cannotOverwrite, FigureMode _figureMode, Vector2 _clearPosition, int _clearSize, int _mapChunkSize)
    {
        int radius = Mathf.FloorToInt(_clearSize / 2);

        if (_figureMode == FigureMode.Square)
        {
            for (int y = 0; y < _mapChunkSize; y++)
            {
                for (int x = 0; x < _mapChunkSize; x++)
                {
                    if (x >= _clearPosition.x - radius && x <= _clearPosition.x + radius && y >= _clearPosition.y - radius && y <= _clearPosition.y + radius)
                    {
                        float distance = x + y - _mapChunkSize + 1;

                        _mapData.noiseMap[x, y] = _mapData.noiseMap[x, y] / (_clearSize / distance);
                        _mapData.obstacleData[x, y].cannotOverwrite = _cannotOverwrite;

                        if(_nodeValue != 0)
                            _mapData.obstacleData[x, y].nodeValue = _nodeValue;
                    }
                }
            }
        }
        else if (_figureMode == FigureMode.Circle)
        {
            _clearSize /= 2;

            for (int y = 0; y < _mapChunkSize; y++)
            {
                for (int x = 0; x < _mapChunkSize; x++)
                {
                    float distance = Vector2.Distance(_clearPosition, new Vector2(x, y));

                    //check if this location is withing the given _clearSize
                    if (distance <= _clearSize)
                    {
                        _mapData.noiseMap[x, y] = _mapData.noiseMap[x, y] / (_clearSize / distance);
                        _mapData.obstacleData[x, y].cannotOverwrite = _cannotOverwrite;
                    }
                }
            }
        }

        return _mapData;
    }

    public MapData ClearCenters(MapData _mapData, int _mapChunkSize)
    {
        float[,] editedNoiseMap = new float[_mapChunkSize, _mapChunkSize];
        System.Array.Copy(_mapData.noiseMap, editedNoiseMap, _mapData.noiseMap.GetLength(0) * _mapData.noiseMap.GetLength(1));

        for (int y = 0; y < _mapChunkSize; y++)
        {
            for (int x = 0; x < _mapChunkSize; x++)
            {
                if (_mapData.noiseMap[x, y] != 0)
                {
                    Vector2 horizontalNeighbours = new Vector2();

                    for (int neighbourX = x - 1; neighbourX <= x + 1; neighbourX += 2)
                    {
                        try
                        {
                            if (_mapData.noiseMap[x, y] != 0)
                            {
                                horizontalNeighbours.x++;
                            }
                        }
                        catch { };
                    }
                    for (int neighbourY = y - 1; neighbourY <= y + 1; neighbourY += 2)
                    {
                        try
                        {
                            if (_mapData.noiseMap[x, y] != 0)
                            {
                                horizontalNeighbours.y++;
                            }
                        }
                        catch { };
                    }

                    if (horizontalNeighbours.x + horizontalNeighbours.y >= 4)
                    {
                        editedNoiseMap[x, y] = 2;
                    }
                    else
                    {
                        _mapData.obstacleData[x, y].isBorder = true;
                        _mapData.obstacleData[x, y].direction = new Vector3(horizontalNeighbours.x - 2, 0, horizontalNeighbours.y - 2);
                    }
                };
            }
        }

        _mapData.noiseMap = editedNoiseMap;
        return _mapData;
    }
}

[System.Serializable]
public struct ObstacleType
{
    public string name;
    public float value;
    public GameObject obstacle;
    public bool cannotOverwrite;
}

public struct ObstacleData
{
    public int nodeValue;
    public bool cannotOverwrite;
    public bool isBorder;
    public Vector3 direction;

    public ObstacleData(int _nodeValue, bool _cannotOverwrite, bool _isBorder, Vector3 _direction)
    {
        this.nodeValue = _nodeValue;
        this.cannotOverwrite = _cannotOverwrite;
        this.isBorder = _isBorder;
        this.direction = _direction;
    }
}