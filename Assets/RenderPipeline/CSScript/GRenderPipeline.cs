using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GRenderPipeline : RenderPipeline
{
    GCameraRender m_cameraRender = new GCameraRender();
    private bool m_useDynamicBatching = false;
    private bool m_useGPUInstance = false;

    public GRenderPipeline(bool useDynamicBatching, bool useGPUInstance, bool useSRPBatcher)
    {
        m_useDynamicBatching = useDynamicBatching;
        m_useGPUInstance = useGPUInstance;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        SortCameras(cameras);
        // Debug.Log(cameras.Length);
        foreach(Camera camera in cameras)
        {
            m_cameraRender.Init(context, camera);
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
}
