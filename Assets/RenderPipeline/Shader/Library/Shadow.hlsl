#ifndef GAME_RENDER_PIPELINE_SHADOW
#define GAME_RENDER_PIPELINE_SHADOW

#define MAX_DIRECTIONAL_LIGHT_SHADOW_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowMap);
// SAMPLER_CMP(sampler_DirectionalShadowMap);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadow)
    float4x4 _DirectionalShadowMatrixs[MAX_DIRECTIONAL_LIGHT_SHADOW_COUNT];
    float4 _DirectionalShadowData[MAX_DIRECTIONAL_LIGHT_SHADOW_COUNT];
CBUFFER_END

float3 TransformWorldToShadowCoord(int index, float3 positionWS)
{
    float4x4 clip = float4x4(
        0.5f, 0.0f, 0.0f, 0.5f,
        0.0f, 0.5f, 0.0f, 0.5f,
        0.0f, 0.0f, 0.5f, 0.5f,
        0.0f, 0.0f, 0.0f, 1.0f
    );

    float4x4 mat = mul(clip, _DirectionalShadowMatrixs[index]);
    return mul(mat, float4(positionWS, 1)).xyz;
}

float SampleDirectionalShadowMap(float3 shadowCoord)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowMap, SHADOW_SAMPLER, shadowCoord);
}

#endif
