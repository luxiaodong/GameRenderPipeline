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
    CullingResults m_cullingResult;
    static ShaderTagId m_unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    public void Init(ScriptableRenderContext context, Camera camera)
    {
        this.m_camera = camera;
        this.m_context = context;
    }

    public void Render()
    {
        if( Cull() == false ) return ;

        m_context.SetupCameraProperties(m_camera);
        m_buffer.ClearRenderTarget(true, true, Color.clear);
        m_buffer.BeginSample(m_bufferName);
        this.ExecuteBuffer();

        this.DrawObject();

        m_buffer.EndSample(m_bufferName);
        this.ExecuteBuffer();

        m_context.Submit();
    }

    private void ExecuteBuffer()
    {
        m_context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
    }

    private bool Cull()
    {
        if(m_camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters) )
        {
            m_cullingResult = m_context.Cull(ref cullingParameters);
            return true;
        }

        return false;
    }

    private void DrawObject()
    {
        var sortingSetting = new SortingSettings(m_camera){criteria = SortingCriteria.CommonOpaque};
        var drawingSetting = new DrawingSettings(m_unlitShaderTagId, sortingSetting);
        var filteringSetting = new FilteringSettings(RenderQueueRange.opaque);
        m_context.DrawRenderers(m_cullingResult, ref drawingSetting, ref filteringSetting);

        m_context.DrawSkybox(m_camera);

        sortingSetting.criteria = SortingCriteria.CommonTransparent;
        drawingSetting.sortingSettings = sortingSetting;
        filteringSetting.renderQueueRange = RenderQueueRange.transparent;
        m_context.DrawRenderers(m_cullingResult, ref drawingSetting, ref filteringSetting);
    }
}
