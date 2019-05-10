Shader "Unlit/NDotV"
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
                float3 normal : NORMAL;
                float4 objectSpacePos : TEXCOORD0;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal;
                o.objectSpacePos = v.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 toCamera = normalize(ObjSpaceViewDir(i.objectSpacePos));
                float nDotV = dot(normalize(i.normal), toCamera);
                fixed4 col = fixed4(nDotV, nDotV, nDotV, 1.0f);
                return col;
            }
            ENDCG
        }
    }
}
