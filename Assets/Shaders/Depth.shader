Shader "Unlit/Depth"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members normal,toCamera)
#pragma exclude_renderers d3d11

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 viewSpacePos : TEXCOORD0;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.viewSpacePos = UnityObjectToViewPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float value = -i.viewSpacePos.z / 20.0f;
                fixed4 col = fixed4(value, value, value, 1.0f);
                return col;
            }
            ENDCG
        }
    }
}
