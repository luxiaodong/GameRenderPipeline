using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class GCameraRender
{
    Camera m_camera;
    ScriptableRenderContext m_context;
    CommandBuffer m_buffer = new CommandBuffer{name = "Render Camera"};
    string m_sampleName = "";
    CullingResults m_cullingResult;
    static ShaderTagId m_unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    GLight m_light = new GLight();
    static ShaderTagId m_litShaderTagId = new ShaderTagId("LitSimple"); //自定义Lit

    ShadowSettings m_shadowSetting;

    public void Init(ScriptableRenderContext context, Camera camera, ShadowSettings shadowSetting)
    {
        m_camera = camera;
        m_context = context;
        m_shadowSetting = shadowSetting;
    }

    public void Render(bool useDynamicBatching, bool useGPUInstance)
    {
        if( Cull(m_shadowSetting.m_maxDistance) == false ) return ;

        PrepareBuffer();

        m_buffer.BeginSample(m_sampleName);
        m_context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
        m_light.Init(m_context, m_cullingResult, m_shadowSetting);
        m_buffer.EndSample(m_sampleName);
        m_context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();

        m_context.SetupCameraProperties(m_camera);
        ClearRenderTarget();
        m_buffer.BeginSample(m_sampleName);
        m_context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();

        DrawObject(useDynamicBatching, useGPUInstance);
        DrawUnsupportedShaders();
        DrawGizmos();
        m_light.Clear();

        m_buffer.EndSample(m_sampleName);
        m_context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
        m_context.Submit();
    }

    void ClearRenderTarget()
    {
        bool clearDepth = true;
        CameraClearFlags flags = m_camera.clearFlags;
        if( flags == CameraClearFlags.Nothing)
        {
            clearDepth = false;
        }

        bool clearColor = false;
        Color color = Color.clear;
        if( flags == CameraClearFlags.Color)
        {
            clearColor = true;
            color = m_camera.backgroundColor.linear;
        }
        
        m_buffer.ClearRenderTarget(clearDepth, clearColor, color);
    }

    bool Cull(float maxShadowDistance)
    {
        if(m_camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters) )
        {
            cullingParameters.shadowDistance = Mathf.Min(maxShadowDistance, m_camera.farClipPlane);
            m_cullingResult = m_context.Cull(ref cullingParameters);
            return true;
        }

        return false;
    }

    void DrawObject(bool useDynamicBatching, bool useGPUInstance)
    {
        var sortingSetting = new SortingSettings(m_camera){criteria = SortingCriteria.CommonOpaque};
        var drawingSetting = new DrawingSettings(m_unlitShaderTagId, sortingSetting){
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstance
        };
        drawingSetting.SetShaderPassName(1, m_litShaderTagId);
        var filteringSetting = new FilteringSettings(RenderQueueRange.opaque);
        m_context.DrawRenderers(m_cullingResult, ref drawingSetting, ref filteringSetting);

        m_context.DrawSkybox(m_camera);

        sortingSetting.criteria = SortingCriteria.CommonTransparent;
        drawingSetting.sortingSettings = sortingSetting;
        filteringSetting.renderQueueRange = RenderQueueRange.transparent;
        m_context.DrawRenderers(m_cullingResult, ref drawingSetting, ref filteringSetting);
    }
}
