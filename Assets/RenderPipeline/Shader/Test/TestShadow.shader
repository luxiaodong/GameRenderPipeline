Shader "GameRenderPipeline/Test/Shadow"
{
    Properties
    {
    }

    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../Library/Common.hlsl"
            #include "../Library/Lighting.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            v2f vert(a2v i)
            {
                v2f o;
                o.positionWS = TransformObjectToWorld(i.positionOS.xyz);
                o.positionCS = TransformObjectToHClip(i.positionOS);
                o.normalWS = TransformObjectToWorldNormal(i.normalOS);
                return o;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                // float3 color = normalize(i.normalWS);
                float3 color = shadow_attenuation(i.positionWS);
                return float4(color, 1.0f);
            }

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags {"LightMode"="ShadowCaster"}
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
            };

            v2f vert(a2v i)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(i.positionOS);

            #if UNITY_REVERSED_Z
                o.positionCS.z = min(o.positionCS.z, o.positionCS.w * UNITY_NEAR_CLIP_VALUE);
            #else
                o.positionCS.z = max(o.positionCS.z, o.positionCS.w * UNITY_NEAR_CLIP_VALUE);
            #endif

                return o;
            }

            void frag(v2f i)
            {
                return ;
            }

            ENDHLSL
        }



    }
}
