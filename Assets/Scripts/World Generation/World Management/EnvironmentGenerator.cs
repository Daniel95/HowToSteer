using UnityEngine;
using System.Collections;

public class EnvironmentGenerator : MonoBehaviour {

    [SerializeField]
    private int smoothSizeDivider = 10;

    [SerializeField]
    private bool randomStartPos = true;

    [SerializeField]
    private EnumTypes.BiomeMode[] biomeModes;

    private int mapTypeSeed;

    void Start()
    {
        if (randomStartPos)
        {
            mapTypeSeed = Random.Range(int.MinValue, int.MaxValue);
            while (GetBiomeMode(new Vector2(0, 0)) != EnumTypes.BiomeMode.Land)
            {
                mapTypeSeed = Random.Range(int.MinValue, int.MaxValue);
            }
        }
        else
        {
            mapTypeSeed = 0;
        }
    }

    public EnumTypes.BiomeMode GetBiomeMode(Vector2 _coords)
    {
        int currBioMode = Mathf.FloorToInt(Mathf.PerlinNoise((_coords.x + mapTypeSeed) / 10, (_coords.y + mapTypeSeed) / 10) * biomeModes.Length);

        return biomeModes[currBioMode];
    }


    public MapData GenerateEnvironment(MapData _mapData, int _mapChunkSize)
    {
        //tweak the first chunk to have a circle in the middle so the player has room to start
        if (_mapData.coordinates == Vector2.zero)
        {
            _mapData = NoiseEditor.FlattenCircleRandomized(_mapData, 0, true, EnumTypes.FigureMode.Circle, new Vector2(_mapChunkSize / 2, _mapChunkSize / 2), _mapChunkSize / 2, _mapChunkSize, 10);
        }

        _mapData = ManageChunkGradient(_mapData, smoothSizeDivider, _mapChunkSize);

        return _mapData;
    }

    public MapData ManageChunkGradient(MapData _mapData, int _sizeDivider, int _mapChunkSize)
    {

        //the swamps adjusts itself to islands
        if (_mapData.levelMode == EnumTypes.BiomeMode.Land)
        {
            for (int i = 0; i < _mapData.directNeighbourCoords.Count; i++)
            {
                //if (MapGenerator.mapDataContainer[_mapData.directNeighbourCoords[i]].levelMode == EnumTypes.BiomeMode.Water)
                if(GetBiomeMode(_mapData.directNeighbourCoords[i]) == EnumTypes.BiomeMode.Water)
                {
                    _mapData = SmoothToDirectNeighbours
                    (
                        _mapData,
                        _mapData.directNeighbourCoords[i] - _mapData.coordinates,
                        _sizeDivider,
                        _mapChunkSize
                    );
                }
            }

            /*
            for (int i = 0; i < _mapData.cornerNeighbourCoords.Count; i++)
            {
                if (MapGenerator.mapDataContainer[_mapData.directNeighbourCoords[i]].levelMode == EnumTypes.BiomeMode.Water)
                {
                    _mapData = SmoothToCornerNeighbours
                    (
                        _mapData,
                        _mapData.directNeighbourCoords[i] - _mapData.coordinates,
                        _sizeDivider,
                        _mapChunkSize
                    );
                }
            }*/
        }

        return _mapData;
    }

    public MapData SmoothToDirectNeighbours(MapData _mapData, Vector2 _dir, int _sizeDivider, int _mapChunkSize)
    {
        if (_dir.x != 0)
        {
            _mapData.noiseMap = NoiseEditor.SmoothSideDirX(_mapData.noiseMap, (int)_dir.x, _sizeDivider, _mapChunkSize);
        }
        if (_dir.y != 0)
        {
            _mapData.noiseMap = NoiseEditor.SmoothSideDirY(_mapData.noiseMap, (int)_dir.y, _sizeDivider, _mapChunkSize);
        }

        return _mapData;
    }

    public MapData SmoothToCornerNeighbours(MapData _mapData, Vector2 _dir, int _sizeDivider, int _mapChunkSize)
    {
        if (_dir.x != 0)
        {
            _mapData.noiseMap = NoiseEditor.SmoothCornerDirX(_mapData.noiseMap, (int)_dir.x, _sizeDivider, _mapChunkSize);
        }
        if (_dir.y != 0)
        {
            _mapData.noiseMap = NoiseEditor.SmoothCornerDirY(_mapData.noiseMap, (int)_dir.y, _sizeDivider, _mapChunkSize);
        }

        return _mapData;
    }
}
