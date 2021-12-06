#ifndef GAME_RENDER_PIPELINE_INPUTDATA
#define GAME_RENDER_PIPELINE_INPUTDATA

struct InputData
{
    float3 positionWS;
    float3 normalWS;
    float3 viewDirectionWS;
    float3 bakedGI;
    float3 shadowCoord;
};

float3 _WorldSpaceCameraPos;

#endif
