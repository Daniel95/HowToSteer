using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UpdateUIText : MonoBehaviour {

    private Text text;

    private string standardMessage;

    void Start() {
        text = GetComponent<Text>();
        standardMessage = text.text;
    }

    public void UpdateText(string _text) {
        text.text = standardMessage + _text;
    }
}
