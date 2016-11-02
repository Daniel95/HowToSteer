using UnityEngine;

public class SwitchActive : MonoBehaviour {

    public void Switch(GameObject _element)
    {
        if (_element.activeSelf)
            _element.SetActive(false);
        else
            _element.SetActive(true);
    }
}
