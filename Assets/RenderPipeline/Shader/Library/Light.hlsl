#ifndef GAME_RENDER_PIPELINE_LIGHT
#define GAME_RENDER_PIPELINE_LIGHT

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

struct Light
{
    float3 position;
    float3 direction;
    float3 color;
};

CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    float3 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float3 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
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

// Light GetTestLight()
// {
//     Light light;
//     light.position = float3(0,1,0);
//     light.direction = _DirectionalLightDirection;
//     light.color = _DirectionalLightColor;
//     return light;
// }

// float3 GetMainLightDirection(Light light)
// {
//     return light.position;
// }

// float3 GetLightDirection(Light light, float3 posWS)
// {
//     return normalize(posWS - light.position);
// }

#endif
