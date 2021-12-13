#ifndef GAME_RENDER_PIPELINE_SHADOW
#define GAME_RENDER_PIPELINE_SHADOW

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

#if defined(_PCF_FILTER_3X3)
	#define PCF_FILTER_SAMPLES 4
	#define PCF_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif defined(_PCF_FILTER_5X5)
	#define PCF_FILTER_SAMPLES 9
	#define PCF_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif defined(_PCF_FILTER_7X7)
	#define PCF_FILTER_SAMPLES 16
	#define PCF_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif

#define MAX_DIRECTIONAL_LIGHT_SHADOW_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowMap);
// SAMPLER_CMP(sampler_DirectionalShadowMap);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadow)
    float4x4 _DirectionalShadowMatrixs[MAX_DIRECTIONAL_LIGHT_SHADOW_COUNT*MAX_CASCADE_COUNT];
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    int _ShadowCascadesCount;
    float3 _ShadowDistanceFade;
    float4 _ShadowMapSize;
CBUFFER_END

struct ShadowData
{
    int cascadeIndex;
    float strength;
};

float Distance(float3 a, float3 b)
{
    return dot(a - b, a - b);
}

ShadowData GetShadowData(float3 positionWS)
{
    float viewZ = -TransformWorldToView(positionWS).z;
    ShadowData data;
    data.cascadeIndex = _ShadowCascadesCount;
    float p = (viewZ - _ShadowDistanceFade.x)/(_ShadowDistanceFade.y - _ShadowDistanceFade.x);
    data.strength = 1.0f - saturate(p);

    int i = 0;
    for(; i < _ShadowCascadesCount; ++i)
    {
        float4 cullingSpheres = _CascadeCullingSpheres[i];
        if( Distance(cullingSpheres.xyz, positionWS) < cullingSpheres.w*cullingSpheres.w )
        {
            data.cascadeIndex = i;
            break;
        }
    }

    if(data.cascadeIndex == _ShadowCascadesCount)
    {
        data.strength = 0.0f;
    }

    return data;
}

float3 TransformWorldToShadowCoord(int lightIndex,int cascadeIndex, float3 positionWS)
{
    float4x4 mat = _DirectionalShadowMatrixs[lightIndex*MAX_CASCADE_COUNT + cascadeIndex];
    return mul(mat, float4(positionWS, 1)).xyz;
}

float SampleDirectionalShadowMap(float3 shadowCoord)
{
#if defined(PCF_FILTER_SAMPLES)
    float weights[PCF_FILTER_SAMPLES];
    float2 positions[PCF_FILTER_SAMPLES];
    // 计算周边采样位置和权重
    PCF_FILTER_SETUP(_ShadowMapSize, shadowCoord.xy, weights, positions);
    float total = 0.0;
    for(int i = 0; i < PCF_FILTER_SAMPLES; ++i)
    {
        float single = SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowMap, SHADOW_SAMPLER, float3(positions[i].xy, shadowCoord.z));
        total += weights[i] * single;
    }

    return total;
#else
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowMap, SHADOW_SAMPLER, shadowCoord);
#endif

// #if defined(_PCF_FILTER_NONE)
// #else
// #endif

}

#endif
