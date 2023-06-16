Shader "Painter/PointCloud"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Pass
        {
            LOD 200

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            uniform float _PointSize;
            fixed4 _Color;

            struct VertexInput
            {
                float4 vertex: POSITION;
                float4 color: COLOR;
            };

            struct VertexOutput
            {
                float4 pos: SV_POSITION;
                float4 col: COLOR;
                float size: PSIZE;
            };

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.col = v.color;

                o.size = _PointSize * (1.0 + random(v.vertex.xy));

                return o;
            }

            float4 frag(VertexOutput o) : COLOR
            {
                return o.col * _Color;
            }
            ENDCG
        }
    }
}