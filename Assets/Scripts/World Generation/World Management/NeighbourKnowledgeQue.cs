using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NeighbourKnowledgeQue : MonoBehaviour {

    private Dictionary<Vector2, MapdataInQueue> GeneratedEnvironmentMapdataInQueueContainer = new Dictionary<Vector2, MapdataInQueue>();
    private Dictionary<Vector2, MapdataInQueue> GeneratedLevelMapdataInQueueContainer = new Dictionary<Vector2, MapdataInQueue>();

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

    //queue until each of our neighbours is done generating its enviroment
    public void StartNeighboursEnviromentQueue(Vector2 _myCoordinates, List<Vector2> _allNeighboursCoords, Action<MapData> _callBack)
    {
        MapData mapdataValue;
        MapdataInQueue mapdataInQueueValue;

        int generatedNeighboursCount = 0;

        for (int i = _allNeighboursCoords.Count - 1; i >= 0; i--)
        {
            //check if my neighbour is generated, if so then increment mine and theirs generatedNeighboursCount
            if (MapGenerator.mapDataContainer.TryGetValue(_allNeighboursCoords[i], out mapdataValue) && MapGenerator.mapDataContainer[_allNeighboursCoords[i]].generatedEnviromentComplete)
            {
                if (GeneratedEnvironmentMapdataInQueueContainer.TryGetValue(_allNeighboursCoords[i], out mapdataInQueueValue))
                {
                    //if the incrementActiveNeighboursCount returns true (which means all neighbours are active), remove it from the list.
                    if (GeneratedEnvironmentMapdataInQueueContainer[_allNeighboursCoords[i]].IncrementActiveNeighboursCount())
                    {
                        GeneratedEnvironmentMapdataInQueueContainer.Remove(_allNeighboursCoords[i]);
                    }
                }

                generatedNeighboursCount++;
            }
        }

        GeneratedEnvironmentMapdataInQueueContainer.Add(_myCoordinates, new MapdataInQueue(_myCoordinates, generatedNeighboursCount, _callBack));
    }

    //queue until each of our neighbours is done generating the level, so that our mesh is up to date with paths or other things from our neighbour
    public void StartNeighboursLevelQueue(Vector2 _myCoordinates, List<Vector2> _allNeighboursCoords, Action<MapData> _callBack)
    {
        MapdataInQueue mapdataInQueueValue;

        int generatedNeighboursCount = 0;

        for (int i = _allNeighboursCoords.Count - 1; i >= 0; i--)
        {
            //check if my neighbour is generated, if so then increment mine and theirs generatedNeighboursCount
            if (MapGenerator.mapDataContainer[_allNeighboursCoords[i]].generatedLevelComplete)
            {
                if (GeneratedLevelMapdataInQueueContainer.TryGetValue(_allNeighboursCoords[i], out mapdataInQueueValue))
                {
                    //if the incrementActiveNeighboursCount returns true (which means all neighbours are active), remove it from the list.
                    if (GeneratedLevelMapdataInQueueContainer[_allNeighboursCoords[i]].IncrementActiveNeighboursCount())
                    {
                        GeneratedLevelMapdataInQueueContainer.Remove(_allNeighboursCoords[i]);
                    }
                }

                generatedNeighboursCount++;
            }
        }

        GeneratedLevelMapdataInQueueContainer.Add(_myCoordinates, new MapdataInQueue(_myCoordinates, generatedNeighboursCount, _callBack));
    }

    public class MapdataInQueue
    {
        public Vector2 coordinates;
        public Action<MapData> callback;
        public int activeNeighboursCount;

        public MapdataInQueue(Vector2 _coordinates, int _activeNeighboursCount, Action<MapData> _callback)
        {
            coordinates = _coordinates;
            activeNeighboursCount = _activeNeighboursCount;
            callback = _callback;
        }

        //returns true when all neighbours are active.
        public bool IncrementActiveNeighboursCount() {
            activeNeighboursCount++;

            return CheckAllNeighboursActive();
        }

        private bool CheckAllNeighboursActive() {
            //check if all our (8) neighbours are generated, if so then activate the callback and leave the queue
            if (activeNeighboursCount >= 8)
            {
                //activate our callback, so that the map/chunk may be further generated
                callback(MapGenerator.mapDataContainer[coordinates]);

                return true;
            }
            return false;
        }
    }
}

