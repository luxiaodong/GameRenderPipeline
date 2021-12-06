#ifndef GAME_RENDER_PIPELINE_SURFACE
#define GAME_RENDER_PIPELINE_SURFACE

struct SurfaceData
{
    float3 albedo;
    float  alpha;
    float  metallic;
    float  smoothness;
    // float3 specular;
    // float3 normalWS;
    float  occlusion;
    float3 emission;
};

#endif
