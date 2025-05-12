Shader "Custom/XRaySilhouetteHDRP"
{
    Properties
    {
        _SilhouetteColor("Silhouette Color", Color) = (0,0,1,1)
        _EmissionPower("Emission Power", Range(1, 20)) = 5
        _FresnelPower("Fresnel Power", Range(0.1, 5)) = 1.5
    }

    HLSLINCLUDE
    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone vulkan metal switch
    
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "XRaySilhouette"
            Tags { "LightMode" = "Forward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always
            ZWrite Off
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _SilhouetteColor;
                float _EmissionPower;
                float _FresnelPower;
            CBUFFER_END
                        
            Varyings Vert(Attributes input)
            {
                Varyings output;
                
                // Vertex transformation
                output.positionCS = TransformObjectToHClip(input.positionOS);
                
                // Calculate normal in world space
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                // Calculate view direction in world space
                float3 positionWS = TransformObjectToWorld(input.positionOS);
                output.viewDirWS = normalize(GetWorldSpaceViewDir(positionWS));
                
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                // Normalize inputs
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                
                // Calculate fresnel effect
                float fresnel = 1.0 - saturate(dot(normalWS, viewDirWS));
                fresnel = pow(fresnel, _FresnelPower);
                
                // Calculate final color with emission and fresnel
                float4 finalColor = _SilhouetteColor * _EmissionPower;
                finalColor.rgb += fresnel * _SilhouetteColor.rgb * _EmissionPower * 0.5;
                finalColor.a = _SilhouetteColor.a;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/InternalErrorShader"
}