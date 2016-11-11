using UnityEngine;
using System.Collections;

public class MouseTerrainTrail : MonoBehaviour {

    [SerializeField]
    private int maxDist = 300;

    private Vector3 mouseWPos;

    private TerrainTrail terrainTrail;

	// Use this for initialization
	void Start () {
        terrainTrail = GetComponent<TerrainTrail>();
        EndlessTerrain endlessTerrain = FindObjectOfType<EndlessTerrain>();
        endlessTerrain.doneLoadingLevel += StartMouseTerrainTrail;

        SendSliderValue sendHeightSliderValue = GameObject.FindGameObjectWithTag(Tags.HeightSlider.ToString()).GetComponent<SendSliderValue>();
        sendHeightSliderValue.OnValueChange += ChangeTrailHeight;
        terrainTrail.lineHeight = (int)sendHeightSliderValue.GetSliderValue();

        SendSliderValue sendWidthSliderValue = GameObject.FindGameObjectWithTag(Tags.WidthSlider.ToString()).GetComponent<SendSliderValue>();
        sendWidthSliderValue.OnValueChange += ChangeTrailWidth;
        terrainTrail.lineWidth = (int)sendWidthSliderValue.GetSliderValue();
    }

    void StartMouseTerrainTrail() {
        terrainTrail.GetPostion += ReturnMouseWPos;
        StartCoroutine(CheckMouseInput());
    }

    void OnDisable() {
        if(FindObjectOfType<EndlessTerrain>() != null)
            FindObjectOfType<EndlessTerrain>().doneLoadingLevel += StartMouseTerrainTrail;

        if (FindObjectOfType<SendSliderValue>() != null)
            FindObjectOfType<SendSliderValue>().OnValueChange -= ChangeTrailHeight;

        terrainTrail.GetPostion -= ReturnMouseWPos;
    }

    private void ChangeTrailHeight(float _newHeight) {

        terrainTrail.lineHeight = (int)_newHeight;
    }

    private void ChangeTrailWidth(float _newWidth) {
        terrainTrail.lineWidth = (int)_newWidth;
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

    IEnumerator CheckMouseInput() {
        while (true) {
            if (!terrainTrail.IsTrailing && Input.GetMouseButton(0))
            {
                terrainTrail.StartTerrainTrails();
            }
            else if (terrainTrail.IsTrailing && Input.GetMouseButtonUp(0))
            {
                terrainTrail.StopTerrainTrails();
            }

            yield return null;
        }
    }

}
