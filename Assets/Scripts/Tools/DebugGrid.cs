using UnityEngine;
using System.Collections.Generic;

public class DebugGrid : MonoBehaviour {

    [SerializeField]
    private int gridSize = 80;

    [SerializeField]
    private Quaternion rotation;

    [SerializeField]
    private int offsetX = -20;

    [SerializeField]
    private int offsetY = 5;

    [SerializeField]
    private GameObject debugText;

    private Dictionary<Vector2, Dictionary<string, GameObject>> debugTextContainer = new Dictionary<Vector2, Dictionary<string, GameObject>>();

    public void SpawnEditableMessage(string _message, Vector2 _coord, string _keyString, int _zOffset = 0) {
        GameObject dText;
        
        if (!debugTextContainer.ContainsKey(_coord))
        {
            debugTextContainer.Add(_coord, new Dictionary<string, GameObject>());
        }
        if (!debugTextContainer[_coord].ContainsKey(_keyString))
        {
            debugTextContainer[_coord].Add(_keyString, dText = Instantiate(debugText, new Vector3(_coord.x * gridSize + offsetX, offsetY, _coord.y * gridSize + _zOffset), debugText.transform.rotation) as GameObject);
            dText.GetComponent<LifeTime>().enabled = false;
        }
        else {
            dText = debugTextContainer[_coord][_keyString];
        }

        dText.GetComponent<TextMesh>().text = _message;
    }

    public void SpawnUniqueMessage(string _message, Vector2 _coord, int _zOffset = 0, bool _shouldDie = true, int _timeTolive = 500) {
        GameObject dText = Instantiate(debugText, new Vector3(_coord.x * gridSize + offsetX, offsetY, _coord.y * gridSize + _zOffset), debugText.transform.rotation) as GameObject;

        LifeTime lifeTime = dText.GetComponent<LifeTime>();
        if (!_shouldDie)
            lifeTime.enabled = false;
        else {
            lifeTime.TimeToLive = _timeTolive;
        }

        dText.GetComponent<TextMesh>().text = _message;
    }
}
