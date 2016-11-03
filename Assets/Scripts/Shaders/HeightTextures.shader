Shader "Custom/HeightTextures" {
	Properties{
		_Offset("HeightOffset", float) = 0
		_TopHeightVal("Top Height Value", float) = 1
		_TopTex("Top Texture", 2D) = "defaulttexture" {}
		_MidHeightVal("Mid Height Value", float) = 0
		_MidTex("Mid Texture", 2D) = "defaulttexture" {}
		_BotHeightVal("Bottom Height Value", float) = -1
		_BotTex("Bottom Texture", 2D) = "defaulttexture" {}

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
			float2 uv_TopTex;
			float2 uv_MidTex;
			float2 uv_BotTex;
		};

		float _Offset;

		float _TopHeightVal;
		float _MidHeightVal;
		float _BotHeightVal;

		sampler2D _TopTex;
		sampler2D _MidTex;
		sampler2D _BotTex;

		half _Glossiness;
		half _Metallic;

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {

			float yWPos = IN.worldPos.y - _Offset;

			float heightValAbove;
			float heightValBelow;
			fixed4 texColorAbove;
			fixed4 texColorBelow;
			
			if (yWPos > _MidHeightVal) {
				heightValAbove = _TopHeightVal;
				heightValBelow = _MidHeightVal;

				texColorAbove = tex2D(_TopTex, IN.uv_TopTex);
				texColorBelow = tex2D(_MidTex, IN.uv_MidTex);
			}
			else {
				heightValAbove = _MidHeightVal;
				heightValBelow = _BotHeightVal;

				texColorAbove = tex2D(_MidTex, IN.uv_MidTex);
				texColorBelow = tex2D(_BotTex, IN.uv_BotTex);
			}

			float difference = yWPos - heightValBelow;
			float maxDifference = heightValAbove - heightValBelow;
			float percentage = difference / maxDifference;

			o.Albedo = lerp(texColorBelow, texColorAbove, percentage);

			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}