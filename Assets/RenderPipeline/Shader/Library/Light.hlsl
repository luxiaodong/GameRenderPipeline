#ifndef GAME_RENDER_PIPELINE_LIGHT
#define GAME_RENDER_PIPELINE_LIGHT

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

struct Light
{
    float3 position;
    float3 direction;
    float3 color;
    float  shadowStrength;
    float  shadowBias;
    float  shadowNormalBias;
};

CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    float3 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float3 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _LightShadowDatas[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

int GetDirectionalLightCount()
{
    return _DirectionalLightCount;
}

Light GetDirectionalLight(int index)
{
    Light light;
    light.direction = _DirectionalLightDirections[index];
    light.color = _DirectionalLightColors[index];
    light.shadowStrength = _LightShadowDatas[index].x;
    light.shadowBias = _LightShadowDatas[index].y;
    light.shadowNormalBias = _LightShadowDatas[index].z;
    return light;
}

Light GetMainLight()
{
    if(_DirectionalLightCount == 0)
    {
        Light light = (Light)0;
        return light;
    }
    
    return GetDirectionalLight(0);
}

#endif
