Shader "Painter/UvIslands"
{
    //how this shader finds what falls in the uv islands:
    //it plots the model in 2d space, so each vertex will plotted at it's uv coordinate
    //the rasterizer will only draw the triangles, which in this case are the same as 
    //the uv islands
    //anything out of this will not be rendered, so the render target will maintain 
    //its original value
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
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
            };

            v2f vert(appdata v)
            {
                v2f o;

                float2 uvRemapped = v.uv.xy;
                uvRemapped = uvRemapped * 2. - 1.;

                o.vertex = float4(uvRemapped.xy, 0., 1.);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target0
            {
                return float4(1., 0., 0., 1.);
            }
            ENDCG
        }

    }
}