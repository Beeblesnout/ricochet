Shader "Beeble/TwoSided"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Specular ("Specular", Range(0,1)) = 0.5
		_Normal("Normal", 2D) = "white" {}
		_NormalScale("Normal Scale", Range(0, 2)) = 1
		_Occlusion("AO", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="AlphaTest" }
		Cull off
		Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        CGPROGRAM
        #pragma surface surf StandardDefaultGI
		#include "UnityPBSLighting.cginc"

		inline half4 LightingStandardDefaultGI(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
		{
			return LightingStandard(s, viewDir, gi);
		}

		inline void LightingStandardDefaultGI_GI(
			SurfaceOutputStandard s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			LightingStandard_GI(s, data, gi);
		}

        sampler2D _MainTex, _Normal, _Occlusion;

        struct Input
        {
            float2 uv_MainTex;
			float2 uv_Normal;
			float2 uv_Occlusion;
        };

        half _Specular, _NormalScale;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
			o.Normal = tex2D(_Normal, IN.uv_Normal) * _NormalScale;
			o.Occlusion = tex2D(_Occlusion, IN.uv_Occlusion);
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
