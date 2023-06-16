// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Painter/TexturePaint"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100
        ZTest Off
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 vertexPosition : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            struct Targets
            {
                fixed4 target0 : SV_Target0;
                fixed4 target1 : SV_Target1;
            };


            sampler2D _PaintTex;

            int _Erase;
            float3 _HitPoint;
            float4 _PaintColor;
            float4x4 o2w;
            float _BrushSize;
            float _BrushSharpness;

            //we create a uv mesh
            //we give it a real world position that spans through our mesh in the xy plane
            //so the final result is, a 2d mesh that has vertices proportional to its uvs
            v2f vert(appdata vertexInput)
            {
                v2f fragmentOutput;

                float2 uv_remapped = vertexInput.uv.xy;
                uv_remapped = uv_remapped * 2. - 1.;
                fragmentOutput.vertex = float4(uv_remapped.xy, 0., 1.);

                fragmentOutput.vertexPosition = mul(o2w, vertexInput.vertex);
                fragmentOutput.uv = vertexInput.uv;

                return fragmentOutput;
            }

            fixed4 alphaBlend(fixed4 c1, fixed4 c2)
            {
                const fixed aa = (1 - c2.a) * c1.a;

                const fixed a = aa + c2.a;
                const fixed r = (aa * c1.r + c2.a * c2.r) / a;
                const fixed g = (aa * c1.g + c2.a * c2.g) / a;
                const fixed b = (aa * c1.b + c2.a * c2.b) / a;

                return fixed4(r, g, b, a);
            }

            //any value of the fragment(v2f in this case) is interpolated between the vertices
            fixed4 frag(v2f fragment) : SV_Target0
            {
                fixed4 paintColor = tex2D(_PaintTex, fragment.uv);

                // return float4(0, 0, 1, 1);
                //the smooth step output is 1 if we are at the brush size or more, and 0 if we are below
                //the minimum, otherwise interpolate between 0 and 1
                //we don't need the bigger d values which produces 1, so we inverse the function
                //if we narrowed down our range (min, max values), this mean we give more 0s and 1s, and less
                //interpolated values, so this increases the hardness of the brush
                const float d = distance(_HitPoint, fragment.vertexPosition);
                const float ss = 1. - smoothstep(_BrushSize * _BrushSharpness, _BrushSize, d);
                // const float4 newColor = _PaintColor * _Erase;
                const float4 newColor = alphaBlend(paintColor, _PaintColor) * _Erase;
                paintColor = lerp(paintColor, newColor, ss);

                // if (d < _BrushSize)
                // paintColor = alphaBlend(paintColor, _PaintColor) * _Erase;

                return paintColor;
            }
            ENDCG
        }

    }
}

// float3 i, j, k, v;
// const float vi = dot(v, i);
// const float vj = dot(v, j);
// const float vk = dot(v, k);
//
// const float ii = dot(i, i);
// const float jj = dot(j, j);
// const float kk = dot(k, k);
//
// const bool c1 = vi > 0. && vi < ii;
// const bool c2 = vj > 0. && vj < jj;
// const bool c3 = vk > 0. && vk < kk;
// if (c1 & c2 && c3)