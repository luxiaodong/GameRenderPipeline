using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; 
using Unity.Collections;

public class GShadow
{
    struct DirectionalLightShadow {
        public int m_visibleLightIndex;
    }

    const string m_bufferName = "Shadow";
    CommandBuffer m_buffer = new CommandBuffer{name = m_bufferName};

    ScriptableRenderContext m_context;
    CullingResults m_cullingResult;
    ShadowSettings m_shadowSetting;

    const int m_maxDirectionalLightShadowCount = 1;
    DirectionalLightShadow[] m_directionalLightShadow = new DirectionalLightShadow[m_maxDirectionalLightShadowCount];
    int m_directionalLightShadowCount = 0;

    static int m_directionalShadowMapPropertyId = Shader.PropertyToID("_DirectionalShadowMap");

    public void Init(ScriptableRenderContext context, CullingResults cullingResult, ShadowSettings shadowSetting)
    {
        m_context = context;
        m_cullingResult = cullingResult;
        m_shadowSetting = shadowSetting;
        // m_buffer.Clear();
        // m_buffer.BeginSample(m_bufferName);
        // //SetShadow();
        // m_buffer.EndSample(m_bufferName);
        // context.ExecuteCommandBuffer(m_buffer);
        // m_buffer.Clear();
    }

    public void SetDirectionalShadowData(int index, LightShadows shadows, float shadowStrength)
    {
        if( m_directionalLightShadowCount < m_maxDirectionalLightShadowCount && 
            shadows != LightShadows.None && 
            m_cullingResult.GetShadowCasterBounds(index, out Bounds b))
        {
            m_directionalLightShadow[m_directionalLightShadowCount] = new DirectionalLightShadow{m_visibleLightIndex = index};
            m_directionalLightShadowCount++;
        }
    }

    public void DrawShadow()
    {
        if(m_directionalLightShadowCount > 0)
        {
            DrawDirectionalShadow();
        }
    }

    void DrawDirectionalShadow()
    {
        int shadowMapSize = (int)m_shadowSetting.m_directional.m_shadowMapSize;
        m_buffer.GetTemporaryRT(m_directionalShadowMapPropertyId, shadowMapSize, shadowMapSize,
			32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        m_buffer.SetRenderTarget(m_directionalShadowMapPropertyId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        m_buffer.ClearRenderTarget(true, false, Color.clear);
        m_buffer.BeginSample(m_bufferName);
        m_context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();

        //for循环?
        DirectionalLightShadow dirLightShadow = m_directionalLightShadow[0];
        var shadowDrawSetting = new ShadowDrawingSettings(m_cullingResult, dirLightShadow.m_visibleLightIndex);
        m_cullingResult.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            dirLightShadow.m_visibleLightIndex, 0, 1, Vector3.zero, shadowMapSize, 0,
            out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData splitData
        );
        shadowDrawSetting.splitData = splitData;
        m_buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
        m_context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
        m_context.DrawShadows(ref shadowDrawSetting);

        m_buffer.EndSample(m_bufferName);
        m_context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
    }

    public void SendShadowDataToShader()
    {}

    public void Clear()
    {
        if(m_directionalLightShadowCount > 0)
        {
            m_buffer.ReleaseTemporaryRT(m_directionalShadowMapPropertyId);
            m_context.ExecuteCommandBuffer(m_buffer);
            m_buffer.Clear();
            m_directionalLightShadowCount = 0;
        }
    }
}
