using UnityEngine;
using System.Collections.Generic;

public class RegionsData : MonoBehaviour {

    [SerializeField]
    private RegionTypes[] regionTypes;

    public static float[] regionHeights;

    public static Color[] regionColors;

    void Awake() {
        List<float> heights = new List<float>();
        List<Color> colors = new List<Color>();

        for (int i = 0; i < regionTypes.Length; i++) {
            heights.Add(regionTypes[i].height);
            colors.Add(regionTypes[i].color);
        }

        regionHeights = heights.ToArray();
        regionColors = colors.ToArray();
    }

    [System.Serializable]
    public struct RegionTypes
    {
        public float height;
        public Color color;
    }
}
