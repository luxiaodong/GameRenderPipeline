#ifndef GAME_RENDER_PIPELINE_COREFUNCTION
#define GAME_RENDER_PIPELINE_COREFUNCTION

#define kDieletricSpec float4(0.04, 0.04, 0.04, 1.0 - 0.04)

real4 unity_SpecCube0_HDR;
half4 _GlossyEnvironmentColor;

TEXTURECUBE(unity_SpecCube0); SAMPLER(samplerunity_SpecCube0);

float3 GlossyEnvironmentReflection(float3 reflectVector, float perceptualRoughness, float occlusion)
{
#if !defined(_ENVIRONMENTREFLECTIONS_OFF)
    float mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
    float4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip);

#if !defined(UNITY_USE_NATIVE_HDR)
    float3 irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
#else
    float3 irradiance = encodedIrradiance.rgb;
#endif

    return irradiance * occlusion;
#endif // GLOSSY_REFLECTIONS

    return _GlossyEnvironmentColor.rgb * occlusion;
}

float OneMinusReflectivityMetallic(float metallic)
{
    float oneMinusDielectricSpec = kDieletricSpec.a;
    return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
}


#endif
