using UnityEngine;
using System.Collections.Generic;
using System;

public static class Paths
{
    //values that we save temporary, will be overwritted each time we make a new path
    private static int mapChunkSize;
    private static int pathWidth;
    private static int heightValue;

    //creates a path that can overlap different chunks
    public static void CreatePathChunksOverflow(Vector2 _startChunkCoords, Vector2 _endChunkCoords, Vector2 _pointA, Vector2 _pointB, int _heightValue, int _pathWidth, float _smoothness, bool _canOverwrite, int _mapChunkSize)
    {
        mapChunkSize = _mapChunkSize - 1;
        pathWidth = _pathWidth;
        heightValue = _heightValue;

        Vector2 chunkOffset = new Vector2(0, 0);

        Vector2 pointVectorDifference = _pointB - _pointA;
        Vector2 chunkVectorDifference = _endChunkCoords - _startChunkCoords;

        int xPosition = 0;
        int yPosition = 0;

        int xLength = Mathf.Abs(Mathf.RoundToInt(pointVectorDifference.x + (chunkVectorDifference.x * mapChunkSize)));
        int yLength = Mathf.Abs(Mathf.RoundToInt(pointVectorDifference.y + (chunkVectorDifference.y * mapChunkSize)));

        int totalPathDistance = Mathf.Abs(Mathf.RoundToInt(xLength)) + Mathf.Abs(yLength);

        Dictionary<Vector2, float> pathExtension = new Dictionary<Vector2, float>();
        if (yLength > xLength)
            pathExtension = GenerateWidthExtension(_smoothness);
        else
            pathExtension = GenerateHeightExtension(_smoothness);

        Dictionary<Vector2, float> pathData = GenerateLineData(_pointA, new Vector2(_pointB.x + (chunkVectorDifference.x * mapChunkSize), _pointB.y - (chunkVectorDifference.y * mapChunkSize)), totalPathDistance);

        foreach (Vector2 coordinates in pathData.Keys)
        {
            //here we check if our position surpassed the positions possible in a chunk.
            //next chunk right
            if (coordinates.x - (chunkOffset.x * mapChunkSize) > mapChunkSize)
            {
                AddWHeightExtensionManually(mapChunkSize, yPosition, pathExtension, chunkOffset, _startChunkCoords);
                chunkOffset.x++;
                AddWHeightExtensionManually(0, yPosition, pathExtension, chunkOffset, _startChunkCoords);
            }
            //next chunk left
            else if (coordinates.x - (chunkOffset.x * mapChunkSize) < 0)
            {
                AddWHeightExtensionManually(0, yPosition, pathExtension, chunkOffset, _startChunkCoords);
                chunkOffset.x--;
                AddWHeightExtensionManually(mapChunkSize, yPosition, pathExtension, chunkOffset, _startChunkCoords);
            }
            //next chunk down
            if (coordinates.y - (chunkOffset.y * mapChunkSize) > mapChunkSize)
            {
                AddWidthExtensionManually(xPosition, mapChunkSize, pathExtension, chunkOffset, _startChunkCoords);
                chunkOffset.y++;
                AddWidthExtensionManually(xPosition, 0, pathExtension, chunkOffset, _startChunkCoords);
            }
            //next chunk up
            else if (coordinates.y - (chunkOffset.y * mapChunkSize) < 0)
            {
                AddWidthExtensionManually(xPosition, 0, pathExtension, chunkOffset, _startChunkCoords);
                chunkOffset.y--;
                AddWidthExtensionManually(xPosition, mapChunkSize, pathExtension, chunkOffset, _startChunkCoords);
            }

            Vector2 currentChunk = _startChunkCoords + new Vector2(chunkOffset.x, chunkOffset.y * -1);

            //from each pathdata point, also apply the width/height extension
            foreach (Vector2 extensionOffset in pathExtension.Keys)
            {
                xPosition = (int)coordinates.x + (int)extensionOffset.x - ((int)chunkOffset.x * mapChunkSize);

                if (xPosition < 0)
                    xPosition = 0;
                else if (xPosition > mapChunkSize)
                    xPosition = mapChunkSize;

                yPosition = (int)coordinates.y + (int)extensionOffset.y - ((int)chunkOffset.y * mapChunkSize);
                if (yPosition < 0)
                    yPosition = 0;
                else if (yPosition > mapChunkSize)
                    yPosition = mapChunkSize;

                //0 is grounded
                //1 is sea lvl
                if (_heightValue == 0)
                {
                    if (MapGenerator.mapDataContainer[currentChunk].noiseMap[xPosition, yPosition] > pathExtension[extensionOffset])
                    {
                        MapGenerator.mapDataContainer[currentChunk].noiseMap[xPosition, yPosition] = pathExtension[extensionOffset];
                    }
                }
                else
                {
                    if (MapGenerator.mapDataContainer[currentChunk].noiseMap[xPosition, yPosition] < pathExtension[extensionOffset])
                    {
                        MapGenerator.mapDataContainer[currentChunk].noiseMap[xPosition, yPosition] = pathExtension[extensionOffset];
                    }
                }
            }
        }

        pathData = null;
        pathExtension = null;
    }

    //generates the local coordinates and heights of the points in the path.
    private static Dictionary<Vector2, float> GenerateLineData(Vector2 _pointA, Vector2 _pointB, int _pathDistance)
    {
        _pathDistance *= 2;

        Dictionary<Vector2, float> pathData = new Dictionary<Vector2, float>();

        float pathIncrementSize = 1f / _pathDistance;

        float value;

        for (float t = 0; t < 1; t += pathIncrementSize)
        {
            Vector2 linePos = Vector2.Lerp(_pointA, _pointB, t);
            Vector2 flooredLinePos = new Vector2(Mathf.FloorToInt(linePos.x), Mathf.FloorToInt(linePos.y));
            Vector2 ceiledLinePos = new Vector2(Mathf.CeilToInt(linePos.x), Mathf.CeilToInt(linePos.y));

            if (!pathData.TryGetValue(ceiledLinePos, out value))
                pathData.Add(ceiledLinePos, heightValue);

            if (!pathData.TryGetValue(flooredLinePos, out value))
                pathData.Add(flooredLinePos, heightValue);
        }

        return pathData;
    }

    //generates the extension of the path to make it higher.
    private static Dictionary<Vector2, float> GenerateHeightExtension(float _smoothness)
    {
        Dictionary<Vector2, float> pathExtensionData = new Dictionary<Vector2, float>();

        if (pathWidth == 0)
            pathWidth = 1;

        float smoothLevel = _smoothness / pathWidth;

        for (Vector2 offset = new Vector2(0, -pathWidth); offset.y <= pathWidth; offset.y++)
        {
            pathExtensionData.Add(offset, CalcHeight((int)offset.y, smoothLevel, heightValue));
        }

        return pathExtensionData;
    }

    //generates the extension of the path to make it wider.
    private static Dictionary<Vector2, float> GenerateWidthExtension(float _smoothness)
    {
        Dictionary<Vector2, float> pathExtensionData = new Dictionary<Vector2, float>();

        if (pathWidth == 0)
            pathWidth = 1;

        float smoothLevel = _smoothness / pathWidth;

        for (Vector2 offset = new Vector2(-pathWidth, 0); offset.x <= pathWidth; offset.x++)
        {
            pathExtensionData.Add(offset, CalcHeight((int)offset.x, smoothLevel, heightValue));
        }

        return pathExtensionData;
    }

    //calc how high a place on the ground should be depeding on how far it is from the path
    private static float CalcHeight(int offset, float _smoothLevel, int _heigthValue) {
        // 0 = land, 1 = water
        float height = _heigthValue;

        float heightOffset = _smoothLevel * Mathf.Abs(offset);

        if (height == 0)
            height += heightOffset;
        else
            height -= heightOffset;

        return height;
    }

    //used for chunk border that might be skipped by a path due to rounding
    private static void AddWidthExtensionManually(int _xPos, int _yPos, Dictionary<Vector2, float> _pathExtension, Vector2 _chunkOffset, Vector2 _startChunkCoords)
    {
        Vector2 originChunkCoord = _startChunkCoords + new Vector2(_chunkOffset.x, _chunkOffset.y * -1);

        if (heightValue == 0)
        {
            foreach (Vector2 _offset in _pathExtension.Keys)
            {
                if (_xPos - _pathExtension.Count - 1 + (int)_offset.x >= 0 && _xPos - pathWidth + (int)_offset.x <= mapChunkSize &&
                    MapGenerator.mapDataContainer[originChunkCoord].noiseMap[_xPos - pathWidth + (int)_offset.x, _yPos] > _pathExtension[_offset]) {
                        MapGenerator.mapDataContainer[originChunkCoord].noiseMap[_xPos - pathWidth + (int)_offset.x, _yPos] = _pathExtension[_offset];
                }
            }
        }
        else {
            foreach (Vector2 _offset in _pathExtension.Keys)
            {
                if (_xPos - _pathExtension.Count - 1 + (int)_offset.x >= 0 && _xPos - pathWidth + (int)_offset.x <= mapChunkSize &&
                    MapGenerator.mapDataContainer[originChunkCoord].noiseMap[_xPos - pathWidth + (int)_offset.x, _yPos] < _pathExtension[_offset]) {
                        MapGenerator.mapDataContainer[originChunkCoord].noiseMap[_xPos - pathWidth + (int)_offset.x, _yPos] = _pathExtension[_offset];
                }
            }
        }
    }

    private static void AddWHeightExtensionManually(int _xPos, int _yPos, Dictionary<Vector2, float> _pathExtension, Vector2 _chunkOffset, Vector2 _startChunkCoords)
    {
        Vector2 originChunkCoord = _startChunkCoords + new Vector2(_chunkOffset.x, _chunkOffset.y * -1);

        if (heightValue == 0)
        {
            foreach (Vector2 _offset in _pathExtension.Keys)
            {
                if (_yPos - pathWidth + (int)_offset.y >= 0 && _yPos + pathWidth + (int)_offset.y <= mapChunkSize &&
                    MapGenerator.mapDataContainer[originChunkCoord].noiseMap[_xPos, _yPos - pathWidth + (int)_offset.y] > _pathExtension[_offset])
                {
                    MapGenerator.mapDataContainer[originChunkCoord].noiseMap[_xPos, _yPos - pathWidth + (int)_offset.y] = _pathExtension[_offset];
                }
            }
        }
        else {
            foreach (Vector2 _offset in _pathExtension.Keys)
            {
                if (_yPos - pathWidth + (int)_offset.y >= 0 && _yPos + pathWidth + (int)_offset.y <= mapChunkSize &&
                    MapGenerator.mapDataContainer[originChunkCoord].noiseMap[_xPos, _yPos - pathWidth + (int)_offset.y] < _pathExtension[_offset])
                {
                    MapGenerator.mapDataContainer[originChunkCoord].noiseMap[_xPos, _yPos - pathWidth + (int)_offset.y] = _pathExtension[_offset];
                }
            }
        }
    }
}
