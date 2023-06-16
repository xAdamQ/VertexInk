Shader "Painter/AlphaBlend"
{
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _BaseTex;

            fixed4 frag(v2f fragment) : SV_Target0
            {
                const float4 baseColor = tex2D(_BaseTex, fragment.uv);
                const float4 overlayColor = tex2D(_MainTex, fragment.uv);

                    // return overlayColor;

                const float alpha = overlayColor.a;

                return fixed4((1 - alpha) * baseColor.xyz + alpha * overlayColor.xyz, 1);
            }
            ENDCG
        }
    }
}