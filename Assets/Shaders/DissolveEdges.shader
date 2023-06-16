Shader "Painter/DissolveEdges"
{

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {

            CGPROGRAM
            #pragma  vertex   vert
            #pragma  fragment frag

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

            sampler2D _MainTex;
            sampler2D _UvIslands;
            uniform float4 _MainTex_TexelSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            //_MainTex_TexelSize.xy = 1 / texture size
            //so it can convert a texture point to uv space
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 actualColor = tex2D(_MainTex, i.uv);
                float uvMap = tex2D(_UvIslands, i.uv);

                // return float4(1, 0, 0, 1);

                if (uvMap.x > .1)
                    return actualColor;


                //this means this color was not visited by the rasterizer and it didn't color it
                int n = 0;
                float4 average = float4(0., 0., 0., 0.);
                const float halfSize = 1.5;

                for (float x = -halfSize; x <= halfSize; x++)
                    for (float y = -halfSize; y <= halfSize; y++)
                    {
                        const float4 adjacentColor = tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(x, y));
                        const float islandR = tex2D(_UvIslands, i.uv + _MainTex_TexelSize.xy * float2(x, y)).r;

                        n += islandR;
                        average += adjacentColor * islandR;
                    }

                average /= n;

                actualColor = average;

                return actualColor;
            }
            ENDCG
        }
    }
}