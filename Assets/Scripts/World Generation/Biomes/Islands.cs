using UnityEngine;
using System.Collections.Generic;

public class Islands : MonoBehaviour {

    [SerializeField]
    private float changeToSpawnIsland = 0.001f;

    [SerializeField]
    private int minIslandRandomizedForm = 4;

    [SerializeField]
    private int maxIslandRandomizedForm = 10;

    [SerializeField]
    private int minRandomConnections = 0;

    [SerializeField]
    private int maxRandomConnections = 4;

    [SerializeField]
    private int pathWidth = 3;

    [SerializeField]
    private float pathSmoothness = 0.5f;

    private List<IslandData> islandsDataContainer = new List<IslandData>();

    public MapData GenerateIslands(MapData _mapData, int _mapChunkSize, float _heigtCurveStartValue)
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

        List<Vector2> landNeighbours = new List<Vector2>();

        for (int i = 0; i < _mapData.directNeighbourCoords.Count; i++) {
            if (MapGenerator.mapDataContainer[_mapData.directNeighbourCoords[i]].levelMode == EnumTypes.BiomeMode.Land) {
                landNeighbours.Add(_mapData.directNeighbourCoords[i]);
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

                    if (oldNoiseMap[x, y] <= 0.5f && !NoiseEditor.CheckOverlap(new Vector2(x, y), randomSize, _mapChunkSize))
                    {
                        if (Random.Range(0, 0.99f) < changeToSpawnIsland)
                        {
                            //_mapData = NoiseEditor.FlattenNoiseArea(_mapData, 2, true, EnumTypes.FigureMode.Circle, new Vector2(x, y), 30, _mapChunkSize);
                            _mapData = NoiseEditor.FlattenCircleRandomized(_mapData, 0, true, EnumTypes.FigureMode.Circle, new Vector2(x, y), randomSize, _mapChunkSize, Random.Range(minIslandRandomizedForm, maxIslandRandomizedForm));
                            obstacleData[x, y].nodeValue = 2;

                            MakeIslandConnection(new Vector2(x,y), pathWidth, _mapData, _mapChunkSize);

                            if (landNeighbours.Count > 0) {
                                MakeLandConnection(new Vector2(x, y), _mapData.coordinates, landNeighbours[Random.Range(0, landNeighbours.Count - 1)], pathWidth, _mapChunkSize);
                            }
                        }
                    }
                }
            }
        }

        _mapData.obstacleData = obstacleData;

        return _mapData;
    }

    private void MakeLandConnection(Vector2 _ourIslandLocalCoordinates, Vector2 _ourChunkCoordinates, Vector2 _destinationChunkCoordinates, int _pathWidth, int _mapChunkSize)
    {
        Vector2 localDestination = new Vector2(Random.Range(0, _mapChunkSize - 1), Random.Range(0, _mapChunkSize - 1));

        MapData destinationMapdata = MapGenerator.mapDataContainer[_destinationChunkCoordinates];
        destinationMapdata = NoiseEditor.FlattenNoiseArea(destinationMapdata, 0, true, EnumTypes.FigureMode.Circle, localDestination, 10, _mapChunkSize);
        MapGenerator.mapDataContainer[_destinationChunkCoordinates] = destinationMapdata;

        Paths.CreatePathChunksOverflow(_ourChunkCoordinates, _destinationChunkCoordinates, _ourIslandLocalCoordinates, localDestination, 0, _pathWidth, pathSmoothness, false, _mapChunkSize);
    }

    private void MakeIslandConnection(Vector2 _ourIslandLocalCoordinates, int _pathWidth, MapData _mapData, int _mapChunkSize)
    {

        int activeConnectionsAmount = 0;
        int maxConnections = Random.Range(minRandomConnections, maxRandomConnections);

        //a dictionary with the distance between us and the other island, and chunkcoordinates of the other island
        Dictionary<float, IslandData> shortestDistanceIslandsData = new Dictionary<float, IslandData>();
        float longestDistanceInDict = 0;

        for (int i = 0; i < islandsDataContainer.Count; i++) {

            if (islandsDataContainer[i].connectionAmount < islandsDataContainer[i].maxConnections)
            {
                for (int n = 0; n < _mapData.allNeighboursCoords.Count; n++)
                {
                    if (islandsDataContainer[i].chunkCoordinates == _mapData.allNeighboursCoords[n])
                    {
                        Vector2 checkingIslandLocalCoordinates = islandsDataContainer[i].localCoordinates;

                        float checkDistance = Vector2.Distance(
                            new Vector2(_ourIslandLocalCoordinates.x + (_mapData.coordinates.x * _mapChunkSize), _ourIslandLocalCoordinates.y + (_mapData.coordinates.y * _mapChunkSize)),
                            new Vector2(checkingIslandLocalCoordinates.x + (islandsDataContainer[i].chunkCoordinates.x * _mapChunkSize), checkingIslandLocalCoordinates.y + (islandsDataContainer[i].chunkCoordinates.y * _mapChunkSize))
                        );

                        //add the first results we get to the list
                        if (shortestDistanceIslandsData.Count < maxConnections)
                        {
                            shortestDistanceIslandsData.Add(checkDistance, islandsDataContainer[i]);

                            //increment our connections amount, and the other islands connections amount
                            activeConnectionsAmount++;
                            islandsDataContainer[i].IncrementConnections();
                        }//then if we get more results, check if they are lower that our highest distance result, and if so replace them
                        else
                        {

                            longestDistanceInDict = FindLongestDistance(shortestDistanceIslandsData);

                            if (checkDistance < longestDistanceInDict)
                            {
                                shortestDistanceIslandsData.Remove(longestDistanceInDict);
                                shortestDistanceIslandsData.Add(checkDistance, islandsDataContainer[i]);
                            }

                        }
                        break;
                    }
                } 
            } else {
                break;
            }
        }

        //then add the paths to our chosen connections
        foreach (float dist in shortestDistanceIslandsData.Keys)
        {
            Paths.CreatePathChunksOverflow(_mapData.coordinates, shortestDistanceIslandsData[dist].chunkCoordinates, _ourIslandLocalCoordinates, shortestDistanceIslandsData[dist].localCoordinates, 0, _pathWidth, pathSmoothness, false, _mapChunkSize);
        }

        if (activeConnectionsAmount < maxConnections)
            islandsDataContainer.Add(new IslandData(activeConnectionsAmount, maxConnections, _ourIslandLocalCoordinates, _mapData.coordinates));
    }

    private float FindLongestDistance(Dictionary<float, IslandData> _shortestDistanceCoords) {
        float longestDist = float.MinValue;

        foreach (float dist in _shortestDistanceCoords.Keys)
        {
            if (dist > longestDist)
                longestDist = dist;
        }
        return longestDist;
    }

    public struct IslandData
    {
        public int connectionAmount;
        public int maxConnections;
        public Vector2 localCoordinates;
        public Vector2 chunkCoordinates;

        public IslandData(int _connectionAmount, int _maxConnections, Vector2 _localCoordinates, Vector2 _chunkCoordinates)
        {
            this.connectionAmount = _connectionAmount;
            this.maxConnections = _maxConnections;
            this.localCoordinates = _localCoordinates;
            this.chunkCoordinates = _chunkCoordinates;
        }

        public void IncrementConnections() {
            connectionAmount++;
        }
    }
}
