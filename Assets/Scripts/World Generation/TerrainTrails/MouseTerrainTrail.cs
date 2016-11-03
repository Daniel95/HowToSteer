using UnityEngine;
using System.Collections;

public class MouseTerrainTrail : MonoBehaviour {

    [SerializeField]
    private int maxDist = 300;

    private Vector3 mouseWPos;

    private TerrainTrail terrainTrail;

	// Use this for initialization
	void Start () {
        FindObjectOfType<MapGenerator>().doneLoadingLevel += StartMouseTerrainTrail;
        terrainTrail = GetComponent<TerrainTrail>();
    }

    void StartMouseTerrainTrail() {
        terrainTrail.GetPostion += ReturnMouseWPos;
        terrainTrail.StartTerrainTrails();
    }

    void OnDisable() {
        if(FindObjectOfType<MapGenerator>() != null)
            FindObjectOfType<MapGenerator>().doneLoadingLevel += StartMouseTerrainTrail;
        terrainTrail.GetPostion -= ReturnMouseWPos;
    }

    public void ChangeTrailHeight(int _newHeight) {
        terrainTrail.lineHeight = _newHeight;
    }

    private Vector3 ReturnMouseWPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDist))
        {
            mouseWPos = hit.point;
        }

        return mouseWPos;
    }
}
