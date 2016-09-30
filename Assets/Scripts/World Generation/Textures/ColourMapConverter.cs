using UnityEngine;
using System.Collections;

public class ColourMapConverter : MonoBehaviour {

    [SerializeField]
    private TerrianType[] colourMapRegions;

    public Color[] GenerateColorMap(float[,] _noiseMap, int _mapChunkSize) {
        //apply the colours
        Color[] colourMap = new Color[_mapChunkSize * _mapChunkSize];
        for (int y = 0; y < _mapChunkSize; y++)
        {
            for (int x = 0; x < _mapChunkSize; x++)
            {
                float currentHeight = _noiseMap[x, y];
                for (int i = 0; i < colourMapRegions.Length; i++)
                {
                    if (currentHeight >= colourMapRegions[i].height)
                    {
                        colourMap[y * _mapChunkSize + x] = colourMapRegions[i].colour;
                    }
                    else {
                        break;
                    }
                }
            }
        }

        return colourMap;
    }
}
