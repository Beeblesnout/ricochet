Shader "Beeble/ExplosionUnlit"
{
    Properties
    {
        _Noise1("Noise 1", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uvl : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 uvl : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _Noise1;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uvl = v.uvl;
                o.color = v.color;
                return o;
                // float x = (v.uv.y);
                // float y = (1-v.uv.x);
                // o.uv = float2(x, y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed a = tex2D(_Noise1, i.uvl.xy);
                half l = i.uvl.z;
                a = smoothstep(l-.01, l+.01, a);

                fixed4 end;
                end.rgb = i.color.rgb;
                end.a = a * i.color.a;
                return end;
            }
            ENDCG
        }
    }
}
