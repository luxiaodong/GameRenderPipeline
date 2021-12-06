using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; 
using Unity.Collections;

public class GShadow
{
    struct DirectionalLightShadow {
        public int m_visibleLightIndex;
        // public float m_strength;
    }

    const string m_bufferName = "Shadow";
    CommandBuffer m_buffer = new CommandBuffer{name = m_bufferName};

    ScriptableRenderContext m_context;
    CullingResults m_cullingResult;
    ShadowSettings m_shadowSetting;

    static int m_directionalShadowMapPropertyId = Shader.PropertyToID("_DirectionalShadowMap");
    static int m_directionalShadowMatrixsPropertyId = Shader.PropertyToID("_DirectionalShadowMatrixs");

    const int m_maxDirectionalLightShadowCount = 1;
    int m_directionalLightShadowCount = 0;
    DirectionalLightShadow[] m_directionalLightShadow = new DirectionalLightShadow[m_maxDirectionalLightShadowCount];
    static Matrix4x4[] m_directionalShadowMatrixs = new Matrix4x4[m_maxDirectionalLightShadowCount];
    static Vector4[] m_directionalShadowData = new Vector4[m_maxDirectionalLightShadowCount];

    public void Init(ScriptableRenderContext context, CullingResults cullingResult, ShadowSettings shadowSetting)
    {
        m_context = context;
        m_cullingResult = cullingResult;
        m_shadowSetting = shadowSetting;
    }

    public void SetDirectionalShadowData(int index, LightShadows shadows)
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

        for(int i =0; i < m_directionalLightShadowCount; ++i)
        {
            DirectionalLightShadow dirLightShadow = m_directionalLightShadow[i];
            var shadowDrawSetting = new ShadowDrawingSettings(m_cullingResult, dirLightShadow.m_visibleLightIndex);
            m_cullingResult.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                dirLightShadow.m_visibleLightIndex, 0, 1, Vector3.zero, shadowMapSize, 0,
                out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData splitData);

            shadowDrawSetting.splitData = splitData;
            m_buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
            m_context.ExecuteCommandBuffer(m_buffer);
            m_buffer.Clear();
            m_context.DrawShadows(ref shadowDrawSetting);

            // m_directionalShadowMatrixs[i] = projMatrix*viewMatrix;
            m_directionalShadowMatrixs[i] = GetShadowTransform(projMatrix, viewMatrix);
            // m_directionalShadowData[i] = new Vector4(dirLightShadow.m_strength,0,0,0);
        }

        m_buffer.EndSample(m_bufferName);
        m_context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
    }

    Matrix4x4 GetShadowTransform(Matrix4x4 proj, Matrix4x4 view)
    {
        if (SystemInfo.usesReversedZBuffer)
        {
            proj.m20 = -proj.m20;
            proj.m21 = -proj.m21;
            proj.m22 = -proj.m22;
            proj.m23 = -proj.m23;
        }

        return proj * view;
    }

    public void SendShadowDataToShader()
    {
        m_buffer.SetGlobalMatrixArray(m_directionalShadowMatrixsPropertyId, m_directionalShadowMatrixs);
        // m_buffer.SetGlobalVectorArray(m_directionalShadowDataPropertyId, m_directionalShadowData);
    }

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
