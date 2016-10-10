using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NeighbourKnowledgeQue : MonoBehaviour {

    void OnDisable()
    {
        StopAllCoroutines();
    }

    //saves all neighbours in mapdata
    public MapData GetAllNeighbours(MapData _mapData) {
        List<Vector2> allNeighbours = new List<Vector2>();

        List<Vector2> directNeighbourCoords = new List<Vector2>();
        List<Vector2> cornerNeighboursCoords = new List<Vector2>();

        int totalCounter = 0;

        //add all neighbours to neighbours to check, and also save who are direct neighbours and who are cornerneighbours
        for (int y = (int)_mapData.coordinates.y - 1; y <= _mapData.coordinates.y + 1; y++)
        {
            for (int x = (int)_mapData.coordinates.x - 1; x <= _mapData.coordinates.x + 1; x++)
            {
                if (_mapData.coordinates != new Vector2(x, y))
                {
                    if (totalCounter % 2 == 0)
                    {
                        cornerNeighboursCoords.Add(new Vector2(x, y));
                    }
                    else
                    {
                        directNeighbourCoords.Add(new Vector2(x, y));
                    }

                    allNeighbours.Add(new Vector2(x, y));
                }

                totalCounter++;
            }
        }

        _mapData.allNeighboursCoords = allNeighbours;
        _mapData.directNeighbourCoords = directNeighbourCoords;
        _mapData.cornerNeighbourCoords = cornerNeighboursCoords;

        return _mapData;
    }

    public void StartAllNeighbourExistQue(Vector2 _myCoordinates, List<Vector2> _allNeighboursCoords, Action<MapData> _callBack)
    {
        StartCoroutine(EnterAllNeighbourExistQue(_myCoordinates, _allNeighboursCoords, _callBack));
    }

    //a que that waits for all neighbours to exists
    IEnumerator EnterAllNeighbourExistQue(Vector2 _myCoordinates, List<Vector2> _allNeighboursCoords, Action<MapData> _callBack)
    {
        bool allSurroundingChunksExist = false;

        //while allsurroundingchunk dont yet exists, loop through coords to check to see if they exist, once all neighbours exist we exit the loop
        while (!allSurroundingChunksExist)
        {
            if (Frames.frames % Frames.framesToSkip == 0)
            {
                allSurroundingChunksExist = true;

                for (int i = _allNeighboursCoords.Count - 1; i >= 0; i--)
                {
                    if (!MapGenerator.mapDataContainer.ContainsKey(_allNeighboursCoords[i]))
                    {
                        allSurroundingChunksExist = false;
                        break;
                    }
                }
            }

            yield return new WaitForFixedUpdate();
        }

        _callBack(MapGenerator.mapDataContainer[_myCoordinates]);
    }

    public void StartAllNeighbourGeneratedLevelQue(Vector2 _myCoordinates, List<Vector2> _allNeighboursCoords, Action<MapData> _callBack)
    {
        StartCoroutine(EnterAllNeighbourGeneratedLevelQue(_myCoordinates, _allNeighboursCoords, _callBack));
    }

    //a que that waits for all neighbours to be generated
    IEnumerator EnterAllNeighbourGeneratedLevelQue(Vector2 _myCoordinates, List<Vector2> _allNeighboursCoords, Action<MapData> _callBack)
    {
        bool allSurroundingChunksExist = false;

        //while allsurroundingchunk dont yet exists, loop through coords to check to see if they exist, once all neighbours exist we exit the loop
        while (!allSurroundingChunksExist)
        {
            if (Frames.frames % Frames.framesToSkip == 0)
            {
                allSurroundingChunksExist = true;

                for (int i = _allNeighboursCoords.Count - 1; i >= 0; i--)
                {
                    if (!MapGenerator.mapDataContainer[_allNeighboursCoords[i]].generatedLevelComplete)
                    {
                        allSurroundingChunksExist = false;
                        break;
                    }
                }
            }

            yield return new WaitForFixedUpdate();
        }

        _callBack(MapGenerator.mapDataContainer[_myCoordinates]);
    }
}