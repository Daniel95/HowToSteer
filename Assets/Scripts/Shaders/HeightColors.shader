Shader "Custom/HeightColors" {
	Properties{
		_Offset("HeightOffset", float) = 0
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows

#pragma target 3.0

		struct Input {
		float3 worldPos;
	};

	float _Offset;

	float _Heights[7];
	float4 _Colors[7];

	half _Glossiness;
	half _Metallic;

	void vert(inout appdata_full v, out Input o) {
		UNITY_INITIALIZE_OUTPUT(Input, o);
	}


	void surf(Input IN, inout SurfaceOutputStandard o) {

		float yWPos = IN.worldPos.y - _Offset;

		//if the height is higher than the highest value or lower then then lowest value, then set its color to its nearest color
		if (yWPos > _Heights[7 - 1]) {
			o.Albedo = _Colors[7 - 1];
		}
		else if (yWPos < _Heights[0]) {
			o.Albedo = _Colors[0];
		}
		else {
			for (int i = 1; i < 7; i++) {
				//if the YPos is between height i - 1 and i
				if (yWPos > _Heights[i - 1] && yWPos < _Heights[i]) {
					//get a color between the last color in the array, and this index color
					//the strength of the color is decided by how close its position is to the heightValue of the color
					float difference = yWPos - _Heights[i - 1];
					float maxDifference = _Heights[i] - _Heights[i - 1];
					float percentage = difference / maxDifference;

					o.Albedo = lerp(_Colors[i - 1], _Colors[i], percentage);
					break;
				}
			}
		}

		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
	}
	ENDCG
	}
		FallBack "Diffuse"
}