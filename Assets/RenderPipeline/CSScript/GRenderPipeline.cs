using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GRenderPipeline : RenderPipeline
{
    GCameraRender m_cameraRender = new GCameraRender();

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        SortCameras(cameras);

        foreach(Camera camera in cameras)
        {
            m_cameraRender.Init(context, camera);
            m_cameraRender.Render();
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
