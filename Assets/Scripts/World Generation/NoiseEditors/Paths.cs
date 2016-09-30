using UnityEngine;
using System.Collections.Generic;
using System;

public static class Paths
{

    //creates a path that can overlap different chunks
    public static void CreatePathChunksOverflow(Vector2 _startChunkCoords, Vector2 _endChunkCoords, Vector2 _pointA, Vector2 _pointB, int _heightValue, int _pathWidth, int _mapChunkSize)
    {
        _mapChunkSize--;

        Vector2 chunkOffset = new Vector2(0, 0);

        Vector2 pointVectorDifference = _pointB - _pointA;
        Vector2 chunkVectorDifference = _endChunkCoords - _startChunkCoords;

        int xPosition = 0;
        int yPosition = 0;

        int xLength = Mathf.Abs(Mathf.RoundToInt(pointVectorDifference.x + (chunkVectorDifference.x * _mapChunkSize)));
        int yLength = Mathf.Abs(Mathf.RoundToInt(pointVectorDifference.y + (chunkVectorDifference.y * _mapChunkSize)));

        bool pathIsHorizontal = true;

        if (yLength > xLength)
            pathIsHorizontal = false;

        int totalPathDistance = Mathf.Abs(Mathf.RoundToInt(xLength)) + Mathf.Abs(yLength);

        Dictionary<Vector2, float> pathWidthExtension = GeneratePathExtension(_pathWidth, _heightValue, pathIsHorizontal);
        Dictionary<Vector2, float> pathData = GeneratePathData(_pointA, new Vector2(_pointB.x + (chunkVectorDifference.x * _mapChunkSize), _pointB.y - (chunkVectorDifference.y * _mapChunkSize)), totalPathDistance, pathWidthExtension);

        foreach (Vector2 coordinates in pathData.Keys)
        {
            //next chunk right
            if ((int)coordinates.x - ((int)chunkOffset.x * _mapChunkSize) > _mapChunkSize)
            {
                foreach (Vector2 _offset in pathWidthExtension.Keys)
                {
                    if(yPosition - _pathWidth + (int)_offset.y > 0 && yPosition - _pathWidth + (int)_offset.y <= _mapChunkSize)
                        MapGenerator.mapDataContainer[_startChunkCoords + new Vector2(chunkOffset.x, chunkOffset.y * -1)].noiseMap[_mapChunkSize, yPosition - _pathWidth + (int)_offset.y] = _heightValue;
                }

                chunkOffset += new Vector2(1, 0);

                foreach (Vector2 _offset in pathWidthExtension.Keys)
                {
                    if (yPosition - _pathWidth + (int)_offset.y > 0 && yPosition - _pathWidth + (int)_offset.y <= _mapChunkSize)
                        MapGenerator.mapDataContainer[_startChunkCoords + new Vector2(chunkOffset.x, chunkOffset.y * -1)].noiseMap[0, yPosition - _pathWidth + (int)_offset.y] = _heightValue;
                }
            }

            //next chunk left
            else if ((int)coordinates.x - ((int)chunkOffset.x * _mapChunkSize) < 0)
            {
                foreach (Vector2 _offset in pathWidthExtension.Keys)
                {
                    if (yPosition - _pathWidth + (int)_offset.y > 0 && yPosition - _pathWidth + (int)_offset.y <= _mapChunkSize)
                        MapGenerator.mapDataContainer[_startChunkCoords + new Vector2(chunkOffset.x, chunkOffset.y * -1)].noiseMap[0, yPosition - _pathWidth + (int)_offset.y] = _heightValue;
                }

                chunkOffset += new Vector2(-1, 0);


                foreach (Vector2 _offset in pathWidthExtension.Keys)
                {
                    if (yPosition - _pathWidth + (int)_offset.y > 0 && yPosition - _pathWidth + (int)_offset.y <= _mapChunkSize)
                        MapGenerator.mapDataContainer[_startChunkCoords + new Vector2(chunkOffset.x, chunkOffset.y * -1)].noiseMap[_mapChunkSize, yPosition - _pathWidth + (int)_offset.y] = _heightValue;
                }
            }

            //next chunk down
            if ((int)coordinates.y - ((int)chunkOffset.y * _mapChunkSize) > _mapChunkSize)
            {
                foreach (Vector2 _offset in pathWidthExtension.Keys)
                {
                    if (xPosition - _pathWidth + (int)_offset.x > 0 && xPosition - _pathWidth + (int)_offset.x <= _mapChunkSize)
                        MapGenerator.mapDataContainer[_startChunkCoords + new Vector2(chunkOffset.x, chunkOffset.y * -1)].noiseMap[xPosition - _pathWidth + (int)_offset.x, _mapChunkSize] = _heightValue;
                }

                chunkOffset += new Vector2(0, 1);

                foreach (Vector2 _offset in pathWidthExtension.Keys)
                {
                    if (xPosition - _pathWidth + (int)_offset.x > 0 && xPosition - _pathWidth + (int)_offset.x <= _mapChunkSize)
                        MapGenerator.mapDataContainer[_startChunkCoords + new Vector2(chunkOffset.x, chunkOffset.y * -1)].noiseMap[xPosition - _pathWidth + (int)_offset.x, 0] = _heightValue;
                }
            }

            //next chunk up
            else if ((int)coordinates.y - ((int)chunkOffset.y * _mapChunkSize) < 0)
            {
                foreach (Vector2 _offset in pathWidthExtension.Keys)
                {
                    if (xPosition - _pathWidth + (int)_offset.x > 0 && xPosition - _pathWidth + (int)_offset.x <= _mapChunkSize)
                        MapGenerator.mapDataContainer[_startChunkCoords + new Vector2(chunkOffset.x, chunkOffset.y * -1)].noiseMap[xPosition - _pathWidth + (int)_offset.x, 0] = _heightValue;
                }

                chunkOffset += new Vector2(0, -1);

                foreach (Vector2 _offset in pathWidthExtension.Keys)
                {
                    if (xPosition - _pathWidth + (int)_offset.x > 0 && xPosition - _pathWidth + (int)_offset.x <= _mapChunkSize)
                        MapGenerator.mapDataContainer[_startChunkCoords + new Vector2(chunkOffset.x, chunkOffset.y * -1)].noiseMap[xPosition - _pathWidth + (int)_offset.x, _mapChunkSize] = 0;
                }
            }


            xPosition = (int)coordinates.x - ((int)chunkOffset.x * _mapChunkSize);
            if (xPosition < 0)
                xPosition = 0;
            else if (yPosition > _mapChunkSize)
                xPosition = _mapChunkSize;

            yPosition = (int)coordinates.y - ((int)chunkOffset.y * _mapChunkSize);
            if (yPosition < 0)
                yPosition = 0;
            else if (yPosition > _mapChunkSize)
                yPosition = _mapChunkSize;

            if (_heightValue == 0) {
                if (MapGenerator.mapDataContainer[_startChunkCoords + new Vector2(chunkOffset.x, chunkOffset.y * -1)].noiseMap[xPosition, yPosition] > pathData[coordinates])
                {
                    MapGenerator.mapDataContainer[_startChunkCoords + new Vector2(chunkOffset.x, chunkOffset.y * -1)].noiseMap[xPosition, yPosition] = pathData[coordinates];
                }
            }
            else
            {
                MapGenerator.mapDataContainer[_startChunkCoords + new Vector2(chunkOffset.x, chunkOffset.y * -1)].noiseMap[xPosition, yPosition] = _heightValue;
            }
        }
    }

    //generates the local coordinates and heights of the points in the path.
    private static Dictionary<Vector2, float> GeneratePathData(Vector2 _pointA, Vector2 _pointB, int _pathDistance, Dictionary<Vector2, float> _pathExtension)
    {
        Dictionary<Vector2, float> pathData = new Dictionary<Vector2, float>();

        float pathIncrementSize = 1f / _pathDistance;

        for (float t = 0; t < 1; t += pathIncrementSize)
        {
            Vector2 linePos = Vector2.Lerp(_pointA, _pointB, t);
            Vector2 flooredLinePos = new Vector2(Mathf.FloorToInt(linePos.x), Mathf.FloorToInt(linePos.y));
            Vector2 ceiledLinePos = new Vector2(Mathf.CeilToInt(linePos.x), Mathf.CeilToInt(linePos.y));

            if (!pathData.ContainsKey(flooredLinePos))
            {
                foreach (Vector2 offset in _pathExtension.Keys)
                {
                    if (!pathData.ContainsKey(flooredLinePos + offset))
                    {
                        pathData.Add(flooredLinePos + offset, _pathExtension[offset]);
                    }
                }
            }

            if (!pathData.ContainsKey(ceiledLinePos))
            {
                foreach (Vector2 offset in _pathExtension.Keys)
                {
                    if (!pathData.ContainsKey(ceiledLinePos + offset))
                    {
                        pathData.Add(ceiledLinePos + offset, _pathExtension[offset]);
                    }
                }
            }
        }

        return pathData;
    }

    //generates the extension of the path to make it wider.
    private static Dictionary<Vector2, float> GeneratePathExtension(int _pathWidth, int _heightValue, bool _horizontalPath)
    {
        Dictionary<Vector2, float> pathExtensionData = new Dictionary<Vector2, float>();

        if (_pathWidth == 0)
            _pathWidth = 1;

        float smoothLevel = 0.5f / _pathWidth;

        if (_horizontalPath)
        {
            for (Vector2 offset = new Vector2(0, -_pathWidth); offset.y <= _pathWidth; offset.y++)
            {
                // 0 = land, 1 = water
                float height = _heightValue;

                float heightOffset = smoothLevel * Mathf.Abs(offset.y);

                if (height == 0)
                    height += heightOffset;
                else
                    height -= heightOffset;

                pathExtensionData.Add(offset, height);
            }
        }
        else
        {
            for (Vector2 offset = new Vector2(-_pathWidth, 0); offset.x <= _pathWidth; offset.x++)
            {
                // 0 = land, 1 = water
                float height = _heightValue;

                float heightOffset = smoothLevel * Mathf.Abs(offset.x);

                if (height == 0)
                    height += heightOffset;
                else
                    height -= heightOffset;

                pathExtensionData.Add(offset, height);
            }
        }

        return pathExtensionData;
    }
}
