Shader "Custom/URP_PinkPurpleVeins"
{
    Properties
    {
        _BaseColor("Base Pink", Color) = (1, 0.8, 0.9, 1)
        _LineColor("Line Purple", Color) = (0.5, 0, 1, 1)
        _Speed("Movement Speed", Float) = 1.0
        _LineWidth("Line Thinness", Range(0.01, 0.5)) = 0.1
        _Frequency("Line Density", Float) = 10.0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _LineColor;
                float _Speed;
                float _LineWidth;
                float _Frequency;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float time = _Time.y * _Speed;
                
                // Add a little wavy distortion to the UVs
                float distortion = sin(IN.uv.y * 5.0 + time) * 0.1;
                
                // Calculate the moving line pattern
                float pattern = sin((IN.uv.x + distortion) * _Frequency + time);
                
                // Sharpen the sine wave into a thin line
                float mask = smoothstep(1.0 - _LineWidth, 1.0, pattern);

                // Blend colors
                return lerp(_BaseColor, _LineColor, mask);
            }
            ENDHLSL
        }
    }
}