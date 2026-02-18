Shader "Custom/URP_VertexColor_Lit"
{
    Properties
    {
        // No textures needed as we are using Vertex Colors
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            // This tag is CRITICAL for URP lighting to work
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Standard URP Includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float3 normalOS    : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float3 normalWS    : TEXCOORD0;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                // Transform position to Clip Space
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                // Pass vertex color
                OUT.color = IN.color;
                // Transform normal to World Space
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Get the main light data (Directional Light)
                Light mainLight = GetMainLight();
                
                // Standard NdotL lighting (Lambert)
                float3 normal = normalize(IN.normalWS);
                float3 lightDir = normalize(mainLight.direction);
                float ndotl = saturate(dot(normal, lightDir));
                
                // Combine Light Color and Vertex Color
                // mainLight.distanceAttenuation handles shadows/falloff
                float3 radiance = mainLight.color * (ndotl * mainLight.distanceAttenuation);
                
                float3 finalColor = IN.color.rgb * radiance;
                
                // Add a small amount of Ambient light so it's not pitch black in shadows
                finalColor += IN.color.rgb * half3(0.1, 0.1, 0.1);

                return half4(finalColor, IN.color.a);
            }
            ENDHLSL
        }
    }
}