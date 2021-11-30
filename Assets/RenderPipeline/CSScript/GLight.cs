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

    public void Init(ScriptableRenderContext context)
    {
        // m_context = context;
        m_buffer.BeginSample(m_bufferName);
        SetDirectionalLight();
        m_buffer.EndSample(m_bufferName);
        context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
    }

    void SetDirectionalLight()
    {
        Light light = RenderSettings.sun;
        m_buffer.SetGlobalVector(m_directionalLightColorPropertyId, light.color);
        m_buffer.SetGlobalVector(m_directionalLightDirectionPropertyId, -light.transform.forward);
    }

}
