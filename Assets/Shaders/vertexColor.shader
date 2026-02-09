Shader "Custom/VertexColor"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float3 normal : NORMAL;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Simple directional lighting
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ndotl = max(0, dot(i.normal, lightDir));
                float lighting = ndotl * 0.5 + 0.5; // Half Lambert
                
                return i.color * lighting;
            }
            ENDCG
        }
    }
}