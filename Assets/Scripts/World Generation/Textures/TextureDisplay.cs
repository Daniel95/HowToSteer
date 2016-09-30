using UnityEngine;
using System.Collections;

public class TextureDisplay : MonoBehaviour {

    [SerializeField]
    private Renderer textureRenderer;

    public void DrawTexture(Texture2D texture) {

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
}
