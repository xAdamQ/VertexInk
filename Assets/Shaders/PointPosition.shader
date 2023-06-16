Shader "Painter/PointPosition"
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

            sampler2D _MainTex;
            float4 _PaintColor;
            float4x4 o2w;
            float4 _RealCenterOffset;
            float _ScaleDownFactor;

            //we create a uv mesh
            //we give it a real world position that spans through our mesh in the xy plane
            //so the final result is, a 2d mesh that has vertices proportional to its uvs
            v2f vert(appdata vertexInput)
            {
                v2f fragmentOutput;

                float2 uv_remapped = vertexInput.uv.xy;
                uv_remapped = uv_remapped * 2. - 1.;
                fragmentOutput.vertex = float4(uv_remapped.xy, 0., 1.);

                fragmentOutput.vertexPosition = (vertexInput.vertex - _RealCenterOffset) * _ScaleDownFactor;

                return fragmentOutput;
            }

            //any value of the fragment(v2f in this case) is interpolated between the vertices
            //I believe this falls in the normalized devices space since the values are between -1 and 1
            float4 frag(v2f fragment) : SV_Target0
            {
                return float4((fragment.vertexPosition + 1.) / 2., 1);
                // return float4((fragment.worldPos + float3(-0.03, -2.54, 0)) * .16, 0.);
                // return float4((fragment.vertexPosition + _RealCenterOffset) * _ScaleDownFactor, 0.);
            }
            ENDCG
        }

    }
}