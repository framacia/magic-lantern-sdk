Shader "OccaSoftware/AutoExposure/FragmentBlitData"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off ZTest Always
        Pass
        {
            Name "AutoExposureFragmentBlitDataPass"

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "AutoExposurePass.hlsl"
            
            struct Attributes
            {
                float4 positionHCS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                float2  uv          : TEXCOORD0;
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                
                
                // Note: The pass is setup with a mesh already in clip
                // space, that's why, it's enough to just output vertex
                // positions
                output.positionCS = float4(input.positionHCS.xyz, 1.0);

                #if UNITY_UV_STARTS_AT_TOP
                output.positionCS.y *= -1;
                #endif
                
                output.uv = input.uv;
                return output;
            }
            
            
            TEXTURE2D(_AutoExposureData);
            TEXTURE2D(_AutoExposureDataPrevious);
            
            half3 Fragment (Varyings input) : SV_Target
            {
                return LOAD_TEXTURE2D(_AutoExposureData, int2(0,0)).rgb;
            }
            ENDHLSL
        }
    }
}