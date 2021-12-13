using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; 
using Unity.Collections;

public class GShadow
{
    struct DirectionalLightShadow {
        public int m_visibleLightIndex;
        public float m_nearPlane;
    }

    const string m_bufferName = "Shadow";
    CommandBuffer m_buffer = new CommandBuffer{name = m_bufferName};

    ScriptableRenderContext m_context;
    CullingResults m_cullingResult;
    ShadowSettings m_shadowSetting;

    static int m_directionalShadowMapPropertyId = Shader.PropertyToID("_DirectionalShadowMap");
    static int m_directionalShadowMatrixsPropertyId = Shader.PropertyToID("_DirectionalShadowMatrixs");
    static int m_shadowCascadesCountPropertyId = Shader.PropertyToID("_ShadowCascadesCount");
    static int m_cascadeCullingSpheresPropertyId = Shader.PropertyToID("_CascadeCullingSpheres");
    // static int m_shadowDistancePropertyId = Shader.PropertyToID("_ShadowDistance");
    static int m_shadowDistanceFadePropertyId = Shader.PropertyToID("_ShadowDistanceFade");
    static int m_shadowMapSizePropertyId = Shader.PropertyToID("_ShadowMapSize");

    static string[] m_pcfFilterKeywords = {
        "_PCF_FILTER_NONE",
        "_PCF_FILTER_3X3",
        "_PCF_FILTER_5X5",
        "_PCF_FILTER_7X7",
    };

    const int m_maxDirectionalLightShadowCount = 4;
    int m_directionalLightShadowCount = 0;
    const int m_maxShadowCascadeCount = 4;
    int m_shadowCascadeCount = 0;
    DirectionalLightShadow[] m_directionalLightShadow = new DirectionalLightShadow[m_maxDirectionalLightShadowCount];
    static Matrix4x4[] m_directionalShadowMatrixs = new Matrix4x4[m_maxDirectionalLightShadowCount*m_maxShadowCascadeCount];
    static Vector4[] m_cascadeCullingSpheres = new Vector4[m_maxShadowCascadeCount];

    public void Init(ScriptableRenderContext context, CullingResults cullingResult, ShadowSettings shadowSetting)
    {
        m_context = context;
        m_cullingResult = cullingResult;
        m_shadowSetting = shadowSetting;
        m_shadowCascadeCount = m_shadowSetting.m_directional.m_cascadeCount;
    }

    public void SetDirectionalShadowData(int index, LightShadows shadows, float nearPlane)
    {
        if( m_directionalLightShadowCount < m_maxDirectionalLightShadowCount && 
            shadows != LightShadows.None && 
            m_cullingResult.GetShadowCasterBounds(index, out Bounds b))
        {
            m_directionalLightShadow[m_directionalLightShadowCount] = new DirectionalLightShadow{
                m_visibleLightIndex = index, m_nearPlane = nearPlane};
                
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

        // 1盏灯一个,最多4盏灯,分割成2x2个
        int splitCount = m_directionalLightShadowCount == 1 ? 1 : 2;
        int tileSize = shadowMapSize/splitCount;
        Vector3 cascadeRatio = new Vector3(
            m_shadowSetting.m_directional.m_cascadeRatio1, 
            m_shadowSetting.m_directional.m_cascadeRatio2,
            m_shadowSetting.m_directional.m_cascadeRatio3);

        for(int i=0; i < m_directionalLightShadowCount; ++i)
        {
            DirectionalLightShadow dirLightShadow = m_directionalLightShadow[i];
            var shadowDrawSetting = new ShadowDrawingSettings(m_cullingResult, dirLightShadow.m_visibleLightIndex);
            Vector2 tileOffset = new Vector2(i%splitCount, i/splitCount);

            int cascadeSplitCount = m_shadowCascadeCount == 1 ? 1 : 2;
            int cascadeTileSize = tileSize/cascadeSplitCount;

            for(int j=0; j < m_shadowCascadeCount; ++j)
            {
                m_cullingResult.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                    dirLightShadow.m_visibleLightIndex, j, m_shadowCascadeCount, cascadeRatio, cascadeTileSize, dirLightShadow.m_nearPlane,
                    out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData splitData);
                shadowDrawSetting.splitData = splitData;

                if(i == 0)
                {
                    m_cascadeCullingSpheres[j] = splitData.cullingSphere;
                }

                Vector2 cascadeOffset = new Vector2(j%cascadeSplitCount, j/cascadeSplitCount);
                Rect r = new Rect(  tileOffset.x*tileSize + cascadeOffset.x*cascadeTileSize,
                                    tileOffset.y*tileSize + cascadeOffset.y*cascadeTileSize,
                                    cascadeTileSize, cascadeTileSize);
                m_buffer.SetViewport(r);
                m_buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);

                m_context.ExecuteCommandBuffer(m_buffer);
                m_buffer.Clear();
                m_context.DrawShadows(ref shadowDrawSetting);

                m_directionalShadowMatrixs[i*m_maxShadowCascadeCount+j] = GetShadowTransform(tileOffset, splitCount, cascadeOffset, cascadeSplitCount, projMatrix, viewMatrix);
            }
        }

        m_buffer.EndSample(m_bufferName);
        m_context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
    }

    Matrix4x4 GetShadowTransform(Vector2 tileOffset, int splitCount, Vector2 cascadeOffset, int cascadeSplitCount, Matrix4x4 proj, Matrix4x4 view)
    {
        if (SystemInfo.usesReversedZBuffer)
        {
            proj.m20 = -proj.m20;
            proj.m21 = -proj.m21;
            proj.m22 = -proj.m22;
            proj.m23 = -proj.m23;
        }

        float s1 = 1.0f/splitCount;
        float s2 = 1.0f/cascadeSplitCount;

        float dx = tileOffset.x*s1 + cascadeOffset.x*s2*s1;
        float dy = tileOffset.y*s1 + cascadeOffset.y*s2*s1;

        Matrix4x4 tile = Matrix4x4.identity;
        tile.SetColumn(0, new Vector4(s1*s2, 0, 0, 0));
        tile.SetColumn(1, new Vector4(0, s1*s2, 0, 0));
        tile.SetColumn(2, new Vector4(0, 0, 1, 0));
        tile.SetColumn(3, new Vector4(dx, dy, 0, 1));

        Matrix4x4 clip = Matrix4x4.identity;
        clip.SetColumn(0, new Vector4(0.5f, 0, 0, 0));
        clip.SetColumn(1, new Vector4(0, 0.5f, 0, 0));
        clip.SetColumn(2, new Vector4(0, 0, 0.5f, 0));
        clip.SetColumn(3, new Vector4(0.5f, 0.5f, 0.5f, 1));

        return  tile * clip * proj * view;
    }

    public void SendShadowDataToShader()
    {
        Vector3 fadeDistance = new Vector3(m_shadowSetting.m_maxDistance*m_shadowSetting.m_fadePercent,m_shadowSetting.m_maxDistance, m_shadowSetting.m_fadePercent);
        m_buffer.SetGlobalVector(m_shadowDistanceFadePropertyId, fadeDistance );
        m_buffer.SetGlobalMatrixArray(m_directionalShadowMatrixsPropertyId, m_directionalShadowMatrixs);
        m_buffer.SetGlobalInt(m_shadowCascadesCountPropertyId, m_shadowCascadeCount);
        m_buffer.SetGlobalVectorArray(m_cascadeCullingSpheresPropertyId, m_cascadeCullingSpheres);

        int shadowMapSize = (int)m_shadowSetting.m_directional.m_shadowMapSize;
        Vector4 mapSize = new Vector4(1.0f/shadowMapSize, 1.0f/shadowMapSize, shadowMapSize, shadowMapSize);
        m_buffer.SetGlobalVector(m_shadowMapSizePropertyId, mapSize);

// _ShadowMapSize
        SetKeywords(m_pcfFilterKeywords, (int)m_shadowSetting.m_directional.m_pcfFilter);
    }

    public void SetKeywords(string[] keywords, int index)
    {
        for(int i = 0; i < keywords.Length; ++i)
        {
            if (i == index)
            {
                m_buffer.EnableShaderKeyword(keywords[i]);
            }
            else
            {
                m_buffer.DisableShaderKeyword(keywords[i]);
            }
        }
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
