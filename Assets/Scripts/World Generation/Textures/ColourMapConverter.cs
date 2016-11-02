
/*using UnityEngine;
using System.Collections;

public class ColourMapConverter : MonoBehaviour
{
    [SerializeField]
    private TerrianType[] textureMapRegions;

    [SerializeField]
    private float detailMultiplier;

    public Color[] GenerateColorMap(float[,] _noiseMap, int _mapChunkSize)
    {
        //apply the colours
        Color[] colourMap = new Color[_mapChunkSize * _mapChunkSize];
        for (int y = 0; y < _mapChunkSize; y++)
        {
            for (int x = 0; x < _mapChunkSize; x++)
            {
                float currentHeight = _noiseMap[x, y];
                for (int i = 1; i < textureMapRegions.Length; i++)
                {
                    if (currentHeight < textureMapRegions[i].height)
                    {
                        colourMap[y * _mapChunkSize + x] = Color.Lerp(textureMapRegions[i - 1].color, textureMapRegions[i].color, currentHeight / textureMapRegions.Length * i);

                        break;
                    }

                    colourMap[y * _mapChunkSize + x] = textureMapRegions[textureMapRegions.Length - 1].color;
                }

            }
        }

        return colourMap;
    }
}

[System.Serializable]
public struct TerrianType
{
    public string name;
    public float height;
    public Color color;
}*/


using UnityEngine;
using System.Collections;

public class ColourMapConverter : MonoBehaviour
{
    [SerializeField]
    private TerrianType[] textureMapRegions;

    [SerializeField]
    private float detailMultiplier;

    public Color[] GenerateColorMap(float[,] _noiseMap, int _mapChunkSize)
    {
        //apply the colours
        Color[] colourMap = new Color[_mapChunkSize * _mapChunkSize];
        for (int y = 0; y < _mapChunkSize; y++)
        {
            for (int x = 0; x < _mapChunkSize; x++)
            {
                float currentHeight = _noiseMap[x, y];
                for (int i = 1; i < textureMapRegions.Length; i++)
                {
                    if (currentHeight < textureMapRegions[i].height)
                    {
                        colourMap[y * _mapChunkSize + x] = Color.Lerp(textureMapRegions[i - 1].texture.GetPixel(x, y), textureMapRegions[i].texture.GetPixel(x, y), currentHeight / textureMapRegions.Length * i);

                        break;
                    }

                    colourMap[y * _mapChunkSize + x] = textureMapRegions[textureMapRegions.Length - 1].texture.GetPixel(x, y);
                }
            }
        }

        return colourMap;
    }
}

[System.Serializable]
public struct TerrianType
{
    public string name;
    public float height;
    public Texture2D texture;
}

/*
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
*/
