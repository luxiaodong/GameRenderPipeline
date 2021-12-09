#ifndef GAME_RENDER_PIPELINE_SHADOW
#define GAME_RENDER_PIPELINE_SHADOW

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
CBUFFER_END

float Distance(float3 a, float3 b)
{
    return dot(a - b, a - b);
}

int FindCascadeIndex(float3 positionWS)
{
    int index = _ShadowCascadesCount - 1;
    for(int i = 0; i < _ShadowCascadesCount; ++i)
    {
        float4 cullingSpheres = _CascadeCullingSpheres[i];
        if( Distance(cullingSpheres.xyz, positionWS) < cullingSpheres.w*cullingSpheres.w )
        {
            index = i;
            break;
        }
    }
    return index;
}

float3 TransformWorldToShadowCoord(int i, float3 positionWS)
{
    int j = FindCascadeIndex(positionWS);
    float4x4 mat = _DirectionalShadowMatrixs[i*MAX_CASCADE_COUNT + j];
    return mul(mat, float4(positionWS, 1)).xyz;
}

float SampleDirectionalShadowMap(float3 shadowCoord)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowMap, SHADOW_SAMPLER, shadowCoord);
}

#endif
