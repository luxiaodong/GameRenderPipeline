using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; 

public class GLight
{
    // ScriptableRenderContext m_context;
    const string m_bufferName = "Lighting";
    CommandBuffer m_buffer = new CommandBuffer{name = m_bufferName};

    static int m_directionalLightColorPropertyId = Shader.PropertyToID("_DirectionalLightColor");
    static int m_directionalLightDirectionPropertyId = Shader.PropertyToID("_DirectionalLightDirection");
    CullingResults m_cullingResults;

    public void Init(ScriptableRenderContext context, CullingResults cullingResult)
    {
        // m_context = context;
        m_cullingResults = cullingResult;
        m_buffer.Clear();
        m_buffer.BeginSample(m_bufferName);
        //SetDirectionalLight();
        SetLight();
        m_buffer.EndSample(m_bufferName);
        context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
    }

    void SetLight()
    {
        NativeArray<VisibleLight> visibleLights = m_cullingResults.visibleLights;
        for (int i = 0; i < visibleLights.Length; i++) 
        {
            VisibleLight visibleLight = visibleLights[i];
            if(visibleLight.lightType == LightType.Directional)
            {
                SetDirectionalLight(visibleLight);
            }
        }
    }

    void SetDirectionalLight(VisibleLight visibleLight)
    {
        Light light = visibleLight.light;
        m_buffer.SetGlobalVector(m_directionalLightColorPropertyId, light.color.linear * light.intensity );
        m_buffer.SetGlobalVector(m_directionalLightDirectionPropertyId, -light.transform.forward);
    }

}
