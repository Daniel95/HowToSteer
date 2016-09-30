using UnityEngine;
using System.Collections;

public class ThrowMoveAbleObject : MonoBehaviour {

    [SerializeField]
    private float strength = 25;

    void OnTriggerEnter(Collider _coll) {
        if (_coll.transform.CompareTag(Tags.Player.ToString()))
        {
            _coll.transform.GetComponent<IMoveAble>().AddMovementY(strength);
            Destroy(this.gameObject);
        }
    }
}
