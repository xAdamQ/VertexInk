Shader "Painter/MergeTextures"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _OverlayTex2 ("Overlay Texture", 2D) = "white" {}
    }

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
                float2 uv : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _OverlayTex;
            sampler2D _OverlayTex2;

            v2f vert(appdata vertexInput)
            {
                v2f fragmentOutput;

                float2 uv_remapped = vertexInput.uv.xy;

                uv_remapped = uv_remapped * 2. - 1.;

                fragmentOutput.vertex = float4(uv_remapped.xy, 0., 1.);
                fragmentOutput.uv = vertexInput.uv;

                return fragmentOutput;
            }

            //any value of the fragment(v2f in this case) is interpolated between the vertices
            fixed4 frag(v2f fragment) : SV_Target
            {
                const float4 baseColor = tex2D(_MainTex, fragment.uv);
                const float4 overlayColor = tex2D(_OverlayTex, fragment.uv);
                const float4 overlayColor2 = tex2D(_OverlayTex2, fragment.uv);

                const float alpha = overlayColor.a;
                const float alpha2 = overlayColor2.a;

                const float4 c1 = (1 - alpha) * baseColor + alpha * overlayColor;
                return (1 - alpha2) * c1 + alpha2 * overlayColor2;
            }
            ENDCG
        }

    }
}