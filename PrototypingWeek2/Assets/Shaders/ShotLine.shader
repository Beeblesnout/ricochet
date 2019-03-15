Shader "Beeble/ShotLine"
{
    Properties
    {
        _UVScaling("UV Scaling", Range(0, 10)) = 1
        _NoiseTex("Noise Texture", 2D) = "white" {}
        // _Life("Lifetime Percent", Range(0, 1)) = 0
        _LineWidth("Line Width", Range(0, .5)) = .25
        _LineBoost("Line Boost", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"  "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            AlphaToMask On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                UNITY_FOG_COORDS(1)
            };

            // fixed4 _Color;
            sampler2D _NoiseTex;
            half _Life, _UVScaling, _LineWidth, _LineBoost;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.color;

                half dispersion = lerp(0, 5, _Life);
                fixed noise = tex2D(_NoiseTex, (i.uv - _Time.x) * _UVScaling) * dispersion;
                _LineWidth *= 1-_Life;
                fixed mask = smoothstep(.5 - _LineWidth + dispersion/2, .5 + _LineWidth + dispersion/2, i.uv.y + noise);
                mask = smoothstep(0, .25 - (.25 * _LineBoost), mask * (1-mask));
                col.a *= mask * (1-_Life);

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
