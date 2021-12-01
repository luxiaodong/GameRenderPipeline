Shader "GameRenderPipeline/Lit/Brdf"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _CutOff ("Alpha Cutoff", Range(0,1)) = 0.5
        [Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
        [Enum(Off,0,On,1)] _ZWrite ("Z Write", float) = 1
    }

    SubShader
    {
        // Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Name "Brdf"
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #pragma multi_compile_instancing
            #pragma shader_feature _CLIPPING

            #include "../Library/Common.hlsl"
            #include "../Library/Lighting.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
                UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
                UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
                UNITY_DEFINE_INSTANCED_PROP(float, _Smoothness)
                UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
                UNITY_DEFINE_INSTANCED_PROP(float, _CutOff)
            UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float3 viewDirectionWS  : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(a2v i)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_TRANSFER_INSTANCE_ID(i, o);
                o.positionWS = TransformObjectToWorld(i.positionOS.xyz);
                o.positionCS = TransformObjectToHClip(i.positionOS);
                o.normalWS = TransformObjectToWorldNormal(i.normalOS);
                float4 mainST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
                o.uv = i.uv * mainST.xy + mainST.zw;
                o.viewDirectionWS = normalize(_WorldSpaceCameraPos - o.positionWS);
                return o;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
                float smoothness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Smoothness);
                float metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Metallic);

                SurfaceData surfaceData;
                surfaceData.albedo = texColor.rgb * baseColor.rgb;
                surfaceData.alpha = texColor.a * baseColor.a;
                surfaceData.smoothness = smoothness;
                surfaceData.metallic = metallic;

            #if defined(_CLIPPING)
                float cutoff = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CutOff);
		        clip(surfaceData.alpha - cutoff);
            #endif

                InputData inputData;
                inputData.positionWS = i.positionWS;
                inputData.normalWS = i.normalWS;
                inputData.viewDirectionWS = i.viewDirectionWS;

                float3 color = float3(0,0,0);
                for(int i=0; i < GetDirectionalLightCount(); ++i)
                {
                    Light light = GetDirectionalLight(i);
                    color += brdf_direct(light, surfaceData, inputData);
                }

                return float4(color, surfaceData.alpha);
            }

            ENDHLSL
        }
    }

    CustomEditor "CustomShaderGUI"
}
