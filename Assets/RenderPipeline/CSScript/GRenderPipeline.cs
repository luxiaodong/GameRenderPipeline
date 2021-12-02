using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GRenderPipeline : RenderPipeline
{
    GCameraRender m_cameraRender = new GCameraRender();
    private bool m_useDynamicBatching = false;
    private bool m_useGPUInstance = false;
    private ShadowSettings m_shadowSetting;

    public GRenderPipeline(bool useDynamicBatching, bool useGPUInstance, bool useSRPBatcher, ShadowSettings shadowSetting)
    {
        m_useDynamicBatching = useDynamicBatching;
        m_useGPUInstance = useGPUInstance;
        m_shadowSetting = shadowSetting;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        SetGlobalShaderConstants();
        SortCameras(cameras);
        // Debug.Log(cameras.Length);
        foreach(Camera camera in cameras)
        {
            m_cameraRender.Init(context, camera, m_shadowSetting);
            m_cameraRender.Render(m_useDynamicBatching, m_useGPUInstance);
        }
    }

    Comparison<Camera> cameraComparison = (camera1, camera2) => { return (int) camera1.depth - (int) camera2.depth; };
    void SortCameras(Camera[] cameras)
    {
        if(cameras.Length > 1)
        {
            Array.Sort(cameras, cameraComparison);
        }
    }

    static private int _GlossyEnvironmentColor = Shader.PropertyToID("_GlossyEnvironmentColor");

    void SetGlobalShaderConstants()
    {
        // When glossy reflections are OFF in the shader we set a constant color to use as indirect specular
        SphericalHarmonicsL2 ambientSH = RenderSettings.ambientProbe;
        Color linearGlossyEnvColor = new Color(ambientSH[0, 0], ambientSH[1, 0], ambientSH[2, 0]) * RenderSettings.reflectionIntensity;
        Color glossyEnvColor = CoreUtils.ConvertLinearToActiveColorSpace(linearGlossyEnvColor);
        Shader.SetGlobalVector(_GlossyEnvironmentColor, glossyEnvColor);
    }

}
