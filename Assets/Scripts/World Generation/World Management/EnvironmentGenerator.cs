using UnityEngine;
using System.Collections;

public class EnvironmentGenerator : MonoBehaviour {

    [SerializeField]
    private int smoothSizeDivider = 10;

    [SerializeField]
    private bool randomStartPos = true;

    [SerializeField]
    private float biomeScale = 25;

    [SerializeField]
    private EnumTypes.BiomeMode[] biomeModes;

    private Vector2 mapTypeSeed;

    void Start()
    {
        if (biomeScale <= 0)
            biomeScale = 0.0001f;

        if (randomStartPos)
        {
            System.Random prng = new System.Random(Random.Range(int.MinValue, int.MaxValue));

            mapTypeSeed.x = prng.Next(-100000, 100000);
            mapTypeSeed.y = prng.Next(-100000, 100000);

            //mapTypeSeed = 
            //change the maptypeseed until we get a seed where V(0,0) is land
            while (GetBiomeMode(new Vector2(0, 0)) != EnumTypes.BiomeMode.Land)
            {
                mapTypeSeed.x = prng.Next(-100000, 100000);
                mapTypeSeed.y = prng.Next(-100000, 100000);
            }
        }
        else
        {
            mapTypeSeed = new Vector2(0, 0);
        }
    }

    public EnumTypes.BiomeMode GetBiomeMode(Vector2 _coords)
    {
        float xVal = (_coords.x + mapTypeSeed.x) / biomeScale;
        float yVal = (_coords.y + mapTypeSeed.y) / biomeScale;

        float perlinValue = Mathf.PerlinNoise(xVal, yVal);
        int currBioMode = Mathf.FloorToInt(Mathf.Abs(perlinValue) * biomeModes.Length);
        return biomeModes[currBioMode];
    }


    public MapData GenerateEnvironment(MapData _mapData, int _mapChunkSize)
    {
        //tweak the first chunk to have a circle in the middle so the player has room to start
        if (_mapData.coordinate == Vector2.zero)
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
                if(GetBiomeMode(_mapData.directNeighbourCoords[i]) == EnumTypes.BiomeMode.Water)
                {
                    _mapData = SmoothToDirectNeighbours
                    (
                        _mapData,
                        _mapData.directNeighbourCoords[i] - _mapData.coordinate,
                        _sizeDivider,
                        _mapChunkSize
                    );
                }
            }

            //code for fixing the corners next to water chunks...
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
