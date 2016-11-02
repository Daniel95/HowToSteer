using UnityEngine;
using System.Collections;

public class SizeController : MonoBehaviour {

    public void ChangeSize(Vector3 _newSize) {
        transform.localScale = _newSize;
    }
}
