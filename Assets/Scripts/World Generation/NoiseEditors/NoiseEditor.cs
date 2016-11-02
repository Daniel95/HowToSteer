using UnityEngine;

public static class NoiseEditor
{

    //edits the noise to dissapear smooth on the X as
    public static float[,] SmoothSideDirX(float[,] _noiseMap, int _dirX, int _sizeDivider, int _mapChunkSize)
    {
        int mapChunkSizeX = _mapChunkSize / _sizeDivider;

        for (int y = 0; y < _mapChunkSize; y++)
        {
            for (int xCounter = 0; xCounter < mapChunkSizeX; xCounter++)
            {
                int x = xCounter;

                //if the dir is inverted, invert the x in the grid (on a symmetric pos)
                if (_dirX > 0)
                    x = _mapChunkSize - xCounter - 1;

                float divider = (float)xCounter / (float)mapChunkSizeX;

                if (divider != 0)
                    _noiseMap[x, y] /= divider;
                else
                    _noiseMap[x, y] = 1;
            }
        }

        return _noiseMap;
    }

    //edits the noise to dissapear smooth on the X as
    public static float[,] SmoothSideDirY(float[,] _noiseMap, int _dirY, int _sizeDivider, int _mapChunkSize)
    {
        int mapChunkSizeY = _mapChunkSize / _sizeDivider;

        for (int yCounter = 0; yCounter < mapChunkSizeY; yCounter++)
        {
            for (int x = 0; x < _mapChunkSize; x++)
            {
                int y = yCounter;

                //if the dir is inverted, invert the x in the grid (on a symmetric pos)
                if (_dirY < 0)
                    y = _mapChunkSize - yCounter - 1;

                float divider = (float)yCounter / (float)mapChunkSizeY;

                if (divider != 0)
                    _noiseMap[x, y] /= divider;
                else
                    _noiseMap[x, y] = 1;
            }
        }

        return _noiseMap;
    }

    //edits the noise to dissapear smooth on the X as
    public static float[,] SmoothCornerDirX(float[,] _noiseMap, int _dirX, int _sizeDivider, int _mapChunkSize)
    {
        int mapChunkSizeX = _mapChunkSize / _sizeDivider;

        for (int y = 0; y < _mapChunkSize; y++)
        {
            for (int xCounter = 0; xCounter < mapChunkSizeX; xCounter++)
            {
                int x = xCounter;

                //if the dir is inverted, invert the x in the grid (on a symmetric pos)
                if (_dirX > 0)
                    x = _mapChunkSize - xCounter - 1;

                float divider = (float)xCounter / (float)mapChunkSizeX;

                if (divider != 0)
                    _noiseMap[x, y] /= divider;
                else
                    _noiseMap[x, y] = 1;
            }
        }

        return _noiseMap;
    }

    //edits the noise to dissapear smooth on the X as
    public static float[,] SmoothCornerDirY(float[,] _noiseMap, int _dirY, int _sizeDivider, int _mapChunkSize)
    {
        int mapChunkSizeY = _mapChunkSize / _sizeDivider;

        for (int yCounter = 0; yCounter < mapChunkSizeY; yCounter++)
        {
            for (int x = 0; x < _mapChunkSize; x++)
            {
                int y = yCounter;

                //if the dir is inverted, invert the x in the grid (on a symmetric pos)
                if (_dirY < 0)
                    y = _mapChunkSize - yCounter - 1;

                float divider = (float)yCounter / (float)mapChunkSizeY;

                if (divider != 0)
                    _noiseMap[x, y] /= divider;
                else
                    _noiseMap[x, y] = 1;
            }
        }

        return _noiseMap;
    }

    //removes all noise from the map to it is flat.
    public static MapData MapBasisRemove(MapData _mapData, int _mapChunkSize)
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

    //flatten a circle, and add other small circles to it to make it seem natural
    public static MapData FlattenCircleRandomized(MapData _mapData, int _nodeValue, bool _cannotOverwrite, EnumTypes.FigureMode _figureMode, Vector2 _clearPosition, int _clearSize, int _mapChunkSize, int _randomizeAmount)
    {

        FlattenNoiseArea(_mapData, _nodeValue, _cannotOverwrite, EnumTypes.FigureMode.Circle, _clearPosition, _clearSize, _mapChunkSize);

        for (int i = 0; i < _randomizeAmount; i++)
        {
            int randomClearSize = Mathf.RoundToInt(_clearSize / Random.Range(1f, 2f));

            Vector2 randomCirclePosition = new Vector2(_clearPosition.x + randomClearSize / 2 * Random.Range(-1f, 1f), _clearPosition.y + randomClearSize / 2 * Random.Range(-1f, 1f));

            _mapData = FlattenNoiseArea(_mapData, _nodeValue, _cannotOverwrite, EnumTypes.FigureMode.Circle, randomCirclePosition, randomClearSize, _mapChunkSize);
        }

        return _mapData;
    }

    
    //replaces an given area
    public static MapData FlattenNoiseArea(MapData _mapData, int _nodeValue, bool _cannotOverwrite, EnumTypes.FigureMode _figureMode, Vector2 _clearPosition, int _clearSize, int _mapChunkSize)
    {
        int radius = Mathf.FloorToInt(_clearSize / 2);

        if (_figureMode == EnumTypes.FigureMode.Square)
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

                        if (_nodeValue != 0)
                            _mapData.obstacleData[x, y].nodeValue = _nodeValue;
                    }
                }
            }
        }
        else if (_figureMode == EnumTypes.FigureMode.Circle)
        {
            _clearSize /= 2;

            for (int y = (int)_clearPosition.y - radius; y <= (int)_clearPosition.y + radius; y++)
            {
                for (int x = (int)_clearPosition.x - radius; x <= (int)_clearPosition.x + radius; x++)
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
    
    public static MapData ClearCenters(MapData _mapData, int _mapChunkSize)
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
