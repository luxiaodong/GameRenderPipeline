#ifndef GAME_RENDER_PIPELINE_LIGHTING
#define GAME_RENDER_PIPELINE_LIGHTING

#include "../Library/Common.hlsl"
#include "../Library/CoreFunction.hlsl"
#include "../Library/Brdf.hlsl"
#include "../Library/Light.hlsl"
#include "../Library/Shadow.hlsl"
#include "../Library/Surface.hlsl"
#include "../Library/InputData.hlsl"

float3 lambert(Light light, SurfaceData surfaceData, InputData inputData)
{
    float ndotl = saturate(dot(inputData.normalWS, light.direction));
    return light.color * surfaceData.albedo * ndotl;
}

float3 brdf_direct(int i, Light light, SurfaceData surfaceData, InputData inputData, bool preMultiAlpha)
{
    float oneMinusReflectivity = OneMinusReflectivityMetallic(surfaceData.metallic);
    float reflectivity = 1.0 - oneMinusReflectivity;
    BrdfData brdfData;
    brdfData.diffuse = surfaceData.albedo * oneMinusReflectivity;
    brdfData.specular = lerp(kDieletricSpec.rgb, surfaceData.albedo, surfaceData.metallic);
    brdfData.perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surfaceData.smoothness);
    brdfData.roughness = max(PerceptualRoughnessToRoughness(brdfData.perceptualRoughness), HALF_MIN);

    if(preMultiAlpha) brdfData.diffuse *= surfaceData.alpha;

    float3 specularTerm = GetUnityBrdf(inputData.normalWS, light.direction, inputData.viewDirectionWS, brdfData.roughness, brdfData.specular);
    float ndotl = saturate(dot(inputData.normalWS, light.direction));
    float3 color = brdfData.diffuse + specularTerm * brdfData.specular;

    float3 deltWS = 0;
    deltWS += light.direction * light.shadowBias;
    deltWS += inputData.normalWS * light.shadowNormalBias * (1.0f - ndotl);
    float3 positionWS = inputData.positionWS + deltWS;

    ShadowData shadowData = GetShadowData(positionWS);
    float attenuation = 1.0f;
    if (shadowData.strength > HALF_MIN)
    {
        inputData.shadowCoord = TransformWorldToShadowCoord(i, shadowData.cascadeIndex, positionWS);
        float shadow = SampleDirectionalShadowMap(inputData.shadowCoord);
        attenuation = lerp(1.0f, shadow, light.shadowStrength * shadowData.strength );
    }

    // attenuation = shadowData.cascadeIndex*0.25f;
    color *= light.color * attenuation * ndotl;
    return color;
}

float3 brdf_indirect(Light light, SurfaceData surfaceData, InputData inputData)
{
    float oneMinusReflectivity = OneMinusReflectivityMetallic(surfaceData.metallic);
    float reflectivity = 1.0 - oneMinusReflectivity;
    BrdfData brdfData;
    brdfData.diffuse = surfaceData.albedo * oneMinusReflectivity;
    brdfData.perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surfaceData.smoothness);
    brdfData.roughness = max(PerceptualRoughnessToRoughness(brdfData.perceptualRoughness), HALF_MIN);
    brdfData.specular = lerp(kDieletricSpec.rgb, surfaceData.albedo, surfaceData.metallic);
    brdfData.grazingTerm = saturate(surfaceData.smoothness + reflectivity);
    brdfData.roughness2 = brdfData.roughness * brdfData.roughness;

    float3 reflectVector = reflect(-inputData.viewDirectionWS, inputData.normalWS);
    float fresnelTerm = Pow4(1.0 - saturate(dot(inputData.normalWS, inputData.viewDirectionWS)));
    float3 diffuse = inputData.bakedGI * brdfData.diffuse;
    float3 specular = GlossyEnvironmentReflection(reflectVector, brdfData.perceptualRoughness, surfaceData.occlusion);
    float specularTerm = 1.0 / (brdfData.roughness2 + 1.0) * lerp(brdfData.specular, brdfData.grazingTerm, fresnelTerm);
    float3 color = diffuse + specularTerm*specular;
    color *= surfaceData.occlusion;
    return color;
    // return _GlossyEnvironmentColor.rgb;
}

float3 shadow_attenuation(float3 positionWS)
{
    float3 shadowCoordWS = TransformWorldToShadowCoord(0, 0, positionWS);
    float shadow = SampleDirectionalShadowMap(shadowCoordWS);
    return shadow*float3(1,1,1);
}

#endif
