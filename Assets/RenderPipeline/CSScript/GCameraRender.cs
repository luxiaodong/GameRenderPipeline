using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GCameraRender
{
    Camera m_camera;
    ScriptableRenderContext m_context;
    const string m_bufferName = "Render Camera";
    CommandBuffer m_buffer = new CommandBuffer{name = m_bufferName};

    public void Init(ScriptableRenderContext context, Camera camera)
    {
        this.m_camera = camera;
        this.m_context = context;
    }

    public void Render()
    {
        m_buffer.ClearRenderTarget(true, true, Color.red);
        m_buffer.BeginSample(m_bufferName);
        this.ExecuteBuffer();

        m_context.SetupCameraProperties(m_camera);
        m_context.DrawSkybox(m_camera);
        
        m_buffer.EndSample(m_bufferName);
        this.ExecuteBuffer();

        m_context.Submit();
    }

    public void ExecuteBuffer()
    {
        m_context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
    }

}
