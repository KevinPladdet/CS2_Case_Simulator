Shader "Custom/WhiteShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LineColor ("Line Color", Color) = (1, 1, 1, 1) // Default to white
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _LineColor;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Sample the texture
                half4 texColor = tex2D(_MainTex, i.uv);

                // Replace color with the specified color while keeping alpha unchanged
                if (texColor.a > 0.0)
                {
                    texColor.rgb = _LineColor.rgb;
                }

                return texColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}