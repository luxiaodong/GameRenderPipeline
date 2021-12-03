using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; 
using Unity.Collections;

public class GLight
{
    ScriptableRenderContext m_context;
    const string m_bufferName = "Light";
    CommandBuffer m_buffer = new CommandBuffer{name = m_bufferName};
    CullingResults m_cullingResult;

    static int m_directionalLightCountPropertyId = Shader.PropertyToID("_DirectionalLightCount");
    static int m_directionalLightColorPropertyId = Shader.PropertyToID("_DirectionalLightColors");
    static int m_directionalLightDirectionPropertyId = Shader.PropertyToID("_DirectionalLightDirections");

    const int m_maxDirectionalLightCount = 4;
    static Vector4[] m_directionalLightColors = new Vector4[m_maxDirectionalLightCount];
    static Vector4[] m_directionalLightDirections = new Vector4[m_maxDirectionalLightCount];

    GShadow m_shadow = new GShadow();

    public void Init(ScriptableRenderContext context, CullingResults cullingResult, ShadowSettings shadowSetting)
    {
        m_context = context;
        m_cullingResult = cullingResult;
        m_buffer.Clear();
        m_buffer.BeginSample(m_bufferName);

        m_shadow.Init(context, cullingResult, shadowSetting);
        SetLightData();
        m_shadow.DrawShadow();

        m_buffer.EndSample(m_bufferName);
        context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
    }

    void SetLightData()
    {
        NativeArray<VisibleLight> visibleLights = m_cullingResult.visibleLights;
        int directionalIndex = 0;
        for (int i = 0; i < visibleLights.Length; i++) 
        {
            VisibleLight visibleLight = visibleLights[i];
            if(visibleLight.lightType == LightType.Directional)
            {
                SetDirectionalLightData(directionalIndex, ref visibleLight);
                m_shadow.SetDirectionalShadowData(directionalIndex, visibleLight.light.shadows, visibleLight.light.shadowStrength);
                directionalIndex++;
                if(directionalIndex == m_maxDirectionalLightCount)
                {
                    break;
                }
            }
        }

        SendLightDataToShader(directionalIndex);
    }

    void SetDirectionalLightData(int index, ref VisibleLight visibleLight)
    {
        Light light = visibleLight.light;
        m_directionalLightColors[index] = light.color.linear * light.intensity;
        m_directionalLightDirections[index] = -light.transform.forward;
        m_directionalLightDirections[index] =  new Vector3(0.3213938f, 0.7660444f, -0.5566705f);
    }

    void SendLightDataToShader(int directionalCount)
    {
        m_buffer.SetGlobalInt(m_directionalLightCountPropertyId, directionalCount);
        m_buffer.SetGlobalVectorArray(m_directionalLightColorPropertyId, m_directionalLightColors);
        m_buffer.SetGlobalVectorArray(m_directionalLightDirectionPropertyId, m_directionalLightDirections);
        m_shadow.SendShadowDataToShader();
    }

    public void Clear()
    {
        m_shadow.Clear();
    }
}
