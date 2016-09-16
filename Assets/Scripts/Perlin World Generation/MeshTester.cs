using UnityEngine;
using System.Collections;

public class MeshTester : MonoBehaviour {

    public AnimationCurve heightCurve;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;

    void Start() {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        meshFilter.sharedMesh = MeshGenerator.GenerateTerrainMesh(Noise.GenerateNoiseMap(10, 10, 20, 15, 5, 0.2f, 2, new Vector2(0, 0), Noise.NormalizeMode.Local), 12, heightCurve, 0).CreateMesh();
    }

    public void DrawMesh(Texture2D _texture2D) {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();

        meshRenderer.sharedMaterial.mainTexture = _texture2D;

        meshFilter.sharedMesh = MeshGenerator.GenerateTerrainMesh(Noise.GenerateNoiseMap(10, 10, 20, 15, 5, 0.2f, 2, new Vector2(0, 0), Noise.NormalizeMode.Local),12, heightCurve, 0).CreateMesh();
        meshCollider.sharedMesh = meshFilter.sharedMesh;// = MeshGenerator.GenerateTerrainMesh(Noise.GenerateNoiseMap(10, 10, 20, 15, 5, 0.2f, 2, new Vector2(0, 0), Noise.NormalizeMode.Local), heightCurve).CreateMesh();
    }
}
