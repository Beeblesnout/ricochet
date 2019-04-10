Shader "Custom/Explosion"
{
    Properties
    {
        _Noise1("Noise 1", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
		Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf BlinnPhong vertex:vert

        #pragma target 3.0

        sampler2D _Noise1;
        float2 uv_Noise1;

        struct Input
        {
			fixed4 color;
            float2 uv;
            half a;
        };

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			// o.color = v.color;
            // o.a = (mul(unity_ObjectToWorld, half3(0, 0, 0)) + 1) / 2;
            // o.a = smoothstep(0, 1, o.a+1);
            // o.color.rgb = o.a*(1-o.a)*2;
            o.color = 1;
            o.uv = UnityObjectToClipPos(v.vertex).xy;
		}

        void surf (Input IN, inout SurfaceOutput o)
        {
            // o.Albedo = tex2D(_Noise1, IN.uv);
            // o.Emission = IN.color.rgb;
            // o.Alpha = IN.color.a;
            o.Alpha = 1;
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
