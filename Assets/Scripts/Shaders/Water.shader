Shader "Custom/Water" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_Speed("Speed", Range(0,20)) = 1
		_Scale("Scale", Range(0,20)) = 1
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}

	SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Opaque" "DisableBatching" = "true" }
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows alpha vertex:vert

		struct Input {
			float2 uv_MainTex;
		};

		float _Scale, _Speed;

		void vert(inout appdata_full v) {
			v.vertex.y += sin((v.vertex.x * v.vertex.x) + (v.vertex.z * v.vertex.z) + (_Time.w * _Speed)) * _Scale;
		}

		sampler2D _MainTex;
		half _Glossiness, _Metallic;
		fixed4 _Color;
		void surf(Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
	}

	ENDCG
	}
	Fallback "Diffuse"
}