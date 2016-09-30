using UnityEngine;
using System.Collections;

public class ChildsClearer : MonoBehaviour {

    public void ClearAllChilds(Transform _transform)
    {
        //clear all old obstacles
        while (_transform.childCount != 0)
        {
            DestroyImmediate(_transform.GetChild(0).gameObject);
        }
    }
}
