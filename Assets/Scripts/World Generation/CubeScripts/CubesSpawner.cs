using UnityEngine;
using System.Collections;

public class CubesSpawner : MonoBehaviour {

    [SerializeField]
    private GameObject nodeObject;

    [SerializeField]
    private float scale = 1;

    private EndlessTerrain endlessTerrain;

    void Start()
    {
        endlessTerrain = GetComponent<EndlessTerrain>();
        GetComponent<ChildsClearer>().ClearAllChilds(transform);
    }

    public void SpawnCubes(MapData _mapData, ObstacleType[] _obstacleTypes, float _heigtCurveStartValue, Vector2 _coord, int _mapChunkSize, bool _generateFromEditor)
    {
        Vector2 offset = _coord * _mapChunkSize - new Vector2(_mapChunkSize / 2, _mapChunkSize / 2);

        Transform parentObject = transform;

        if (_generateFromEditor) {
            GetComponent<ChildsClearer>().ClearAllChilds(transform);
        }
        else {
            //if we are not in the editor, the parentObject is the plane from endlessTerrain
            parentObject = endlessTerrain.GetTerrainChunkTransfrom(_coord);
        }

        for (int y = 0; y < _mapChunkSize; y++)
        {
            for (int x = 0; x < _mapChunkSize; x++)
            {
                if (_mapData.noiseMap[x, y] >= _heigtCurveStartValue)
                {
                    if (_mapData.obstacleData[x, y].nodeValue < 1)
                    {
                        GameObject obstacle = Instantiate(nodeObject, new Vector3((x + offset.x) * scale, nodeObject.transform.localScale.y / 2, (y + offset.y) * scale), new Quaternion()) as GameObject;
                        obstacle.transform.parent = parentObject;
                    }
                    else
                    {
                        GameObject obstacle = Instantiate(_obstacleTypes[_mapData.obstacleData[x, y].nodeValue].obstacle, new Vector3((x + offset.x) * scale, _obstacleTypes[_mapData.obstacleData[x, y].nodeValue].obstacle.transform.localScale.y / 2, (y + offset.y) * scale), new Quaternion()) as GameObject;
                        obstacle.transform.parent = parentObject;
                    }
                }
            }
        }
    }
}
