using UnityEngine;
using System.Collections.Generic;

public class Islands : MonoBehaviour {

    [SerializeField]
    private int minIslandsCount = 1;

    [SerializeField]
    private int maxIslandsCount = 7;

    [SerializeField]
    private int minIslandsSize = 15;

    [SerializeField]
    private int maxIslandSize = 35;

    [SerializeField]
    private int minIslandRandomizedForm = 4;

    [SerializeField]
    private int maxIslandRandomizedForm = 10;

    [SerializeField]
    private int maxSearchableIslands = 3;

    [SerializeField]
    private int minRandomConnections = 0;

    [SerializeField]
    private int maxRandomConnections = 4;

    [SerializeField]
    private float maxAxisDifference = 45;

    [SerializeField]
    private int pathWidth = 3;

    [SerializeField]
    private float pathSmoothness = 0.5f;

    private Dictionary<Vector2, List<IslandData>> islandsChunkContainer = new Dictionary<Vector2, List<IslandData>>();

    public MapData GenerateIslands(MapData _mapData, int _mapChunkSize, float _heigtCurveStartValue)
    {
        ObstacleData[,] obstacleData = new ObstacleData[_mapChunkSize, _mapChunkSize];

        float[,] oldNoiseMap = new float[_mapChunkSize, _mapChunkSize];
        System.Array.Copy(_mapData.noiseMap, oldNoiseMap, _mapData.noiseMap.GetLength(0) * _mapData.noiseMap.GetLength(1));

        //edits the noise to be completely sea
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

        islandsChunkContainer.Add(_mapData.coordinate, new List<IslandData>());

        //spawn a random amount of island at random locations in the chunk.
        int randomIslandCount = Random.Range(minIslandsCount, maxIslandsCount);
        for (int i = 0; i < randomIslandCount; i++) {
            int randomSize = Random.Range(minIslandsSize, maxIslandSize);

            Vector2 spawnPos = new Vector2(Random.Range(randomSize, _mapChunkSize - randomSize), Random.Range(randomSize, _mapChunkSize - randomSize));

            _mapData = NoiseEditor.FlattenCircleRandomized(_mapData, 0, true, EnumTypes.FigureMode.Circle, spawnPos, randomSize, _mapChunkSize, Random.Range(minIslandRandomizedForm, maxIslandRandomizedForm));
            obstacleData[(int)spawnPos.x, (int)spawnPos.y].nodeValue = 2;

            MakeIslandConnections(spawnPos, pathWidth, _mapData, _mapChunkSize);

            if (landNeighbours.Count > 0)
            {
                MakeLandConnection(spawnPos, _mapData.coordinate, landNeighbours[Random.Range(0, landNeighbours.Count - 1)], pathWidth, _mapChunkSize);
            }
        }

        _mapData.obstacleData = obstacleData;

        return _mapData;
    }

    private void MakeLandConnection(Vector2 _ourIslandLocalCoordinates, Vector2 _ourChunkCoordinates, Vector2 _destinationChunkCoordinates, int _pathWidth, int _mapChunkSize)
    {
        int destinationClearSize = 10;

        Vector2 localDestination = new Vector2(Random.Range(destinationClearSize, _mapChunkSize - destinationClearSize), Random.Range(destinationClearSize, _mapChunkSize - destinationClearSize));

        MapData destinationMapdata = MapGenerator.mapDataContainer[_destinationChunkCoordinates];
        destinationMapdata = NoiseEditor.FlattenNoiseArea(destinationMapdata, 0, true, EnumTypes.FigureMode.Circle, localDestination, destinationClearSize, _mapChunkSize);
        MapGenerator.mapDataContainer[_destinationChunkCoordinates] = destinationMapdata;

        Paths.CreatePathChunksOverflow(_ourChunkCoordinates, _destinationChunkCoordinates, _ourIslandLocalCoordinates, localDestination, 0, _pathWidth, pathSmoothness, false, _mapChunkSize);
    }

    private void MakeIslandConnections(Vector2 _ourIslandLocalCoordinates, int _pathWidth, MapData _mapData, int _mapChunkSize)
    {
        int activeConnectionsAmount = 0;

        //a dictionary with the distance between us and the other island, and chunkcoordinates of the other island
        Dictionary<float, IslandData> shortestDistanceIslandsData = new Dictionary<float, IslandData>();
        float longestDistanceInDict = 0;

        for (int n = 0; n < _mapData.allNeighboursCoords.Count; n++) {
            if (islandsChunkContainer.ContainsKey(_mapData.allNeighboursCoords[n])) {
                Vector2 chunkOffset = _mapData.coordinate - _mapData.allNeighboursCoords[n];

                for (int i = 0; i < islandsChunkContainer[_mapData.allNeighboursCoords[n]].Count; i++)
                {
                    IslandData otherIsland = islandsChunkContainer[_mapData.allNeighboursCoords[n]][i];

                    Vector2 vertorDifference = _ourIslandLocalCoordinates - otherIsland.localCoordinates + new Vector2(chunkOffset.x * _mapChunkSize, chunkOffset.y * _mapChunkSize);

                    if (Mathf.Abs(vertorDifference.x) < maxAxisDifference || Mathf.Abs(vertorDifference.y) < maxAxisDifference)
                    {
                        float dist = Vector2.Distance(
                            new Vector2(_ourIslandLocalCoordinates.x + (_mapData.coordinate.x * _mapChunkSize), _ourIslandLocalCoordinates.y + (_mapData.coordinate.y * _mapChunkSize)),
                            new Vector2(otherIsland.localCoordinates.x + (otherIsland.chunkCoordinates.x * _mapChunkSize), otherIsland.localCoordinates.y + (otherIsland.chunkCoordinates.y * _mapChunkSize))
                        );

                        if (!shortestDistanceIslandsData.ContainsKey(dist))
                        {

                            //add the first few results we get to the list
                            if (shortestDistanceIslandsData.Count < maxSearchableIslands)
                            {

                                shortestDistanceIslandsData.Add(dist, otherIsland);

                                //increment our connections amount, and the other islands connections amount
                                activeConnectionsAmount++;
                                otherIsland.IncrementConnections();
                            }//then if we get more results, check if they are lower that our highest distance result, and if so replace them
                            else
                            {
                                longestDistanceInDict = FindLongestDistance(shortestDistanceIslandsData);

                                if (dist < longestDistanceInDict)
                                {
                                    shortestDistanceIslandsData.Remove(longestDistanceInDict);
                                    shortestDistanceIslandsData.Add(dist, otherIsland);
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }

        //randomconnectionamout is random between chosen min and max, and cannot be greater than maxSearchableIslands
        int randomConnectionsAmount = Random.Range(minRandomConnections, maxRandomConnections);

        if (shortestDistanceIslandsData.Count > 0)
        {
            if (randomConnectionsAmount > maxSearchableIslands)
                randomConnectionsAmount = maxSearchableIslands;

            //out of all the closest islands (limited maxSearchableIsland), a random amount of of random connections are picked.
            List<float> closestIslandsKeysleftOvers = new List<float>(shortestDistanceIslandsData.Keys);

            for (int i = 0; i < randomConnectionsAmount; i++)
            {
                int randomIndex = Random.Range(0, closestIslandsKeysleftOvers.Count);

                Paths.CreatePathChunksOverflow(_mapData.coordinate, shortestDistanceIslandsData[closestIslandsKeysleftOvers[randomIndex]].chunkCoordinates, _ourIslandLocalCoordinates, shortestDistanceIslandsData[closestIslandsKeysleftOvers[randomIndex]].localCoordinates, 0, _pathWidth, pathSmoothness, false, _mapChunkSize);
                closestIslandsKeysleftOvers.Remove(randomIndex);
            }
        }
        if (activeConnectionsAmount < randomConnectionsAmount)
            islandsChunkContainer[_mapData.coordinate].Add(new IslandData(activeConnectionsAmount, randomConnectionsAmount, _ourIslandLocalCoordinates, _mapData.coordinate));
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

    public void RemoveIslandData(Vector2 _coord) {
        islandsChunkContainer.Remove(_coord);
    } 

    public class IslandsChunk
    {
        public List<IslandData> islands = new List<IslandData>();

        public void AddIsland(IslandData _island)
        {
            islands.Add(_island);
        }
    }

    public struct IslandData
    {
        public int connectionAmount;
        public int maxConnections;
        public Vector2 localCoordinates;
        public Vector2 chunkCoordinates;

        public IslandData(int _connectionAmount, int _maxConnections, Vector2 _localCoordinates, Vector2 _chunkCoordinates)
        {
            connectionAmount = _connectionAmount;
            maxConnections = _maxConnections;
            localCoordinates = _localCoordinates;
            chunkCoordinates = _chunkCoordinates;
        }

        public void IncrementConnections() {
            connectionAmount++;
        }
    }
}