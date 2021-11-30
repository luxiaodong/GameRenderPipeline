using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; 
using Unity.Collections;

public class GLight
{
    // ScriptableRenderContext m_context;
    const string m_bufferName = "Lighting";
    CommandBuffer m_buffer = new CommandBuffer{name = m_bufferName};
    CullingResults m_cullingResults;

    static int m_directionalLightCountPropertyId = Shader.PropertyToID("_DirectionalLightCount");
    static int m_directionalLightColorPropertyId = Shader.PropertyToID("_DirectionalLightColors");
    static int m_directionalLightDirectionPropertyId = Shader.PropertyToID("_DirectionalLightDirections");

    const int m_maxDirectionalLightCount = 4;
    static Vector4[] m_directionalLightColors = new Vector4[m_maxDirectionalLightCount];
    static Vector4[] m_directionalLightDirections = new Vector4[m_maxDirectionalLightCount];

    public void Init(ScriptableRenderContext context, CullingResults cullingResult)
    {
        // m_context = context;
        m_cullingResults = cullingResult;
        m_buffer.Clear();
        m_buffer.BeginSample(m_bufferName);
        // SetDirectionalLight();
        SetLight();
        m_buffer.EndSample(m_bufferName);
        context.ExecuteCommandBuffer(m_buffer);
        m_buffer.Clear();
    }

    void SetLight()
    {
        NativeArray<VisibleLight> visibleLights = m_cullingResults.visibleLights;
        int directionalIndex = 0;
        for (int i = 0; i < visibleLights.Length; i++) 
        {
            VisibleLight visibleLight = visibleLights[i];
            if(visibleLight.lightType == LightType.Directional)
            {
                SetDirectionalLight(directionalIndex, ref visibleLight);
                directionalIndex++;

                if(directionalIndex == m_maxDirectionalLightCount)
                {
                    break;
                }
            }
        }

        m_buffer.SetGlobalInt(m_directionalLightCountPropertyId, directionalIndex);
        m_buffer.SetGlobalVectorArray(m_directionalLightColorPropertyId, m_directionalLightColors);
        m_buffer.SetGlobalVectorArray(m_directionalLightDirectionPropertyId, m_directionalLightDirections);
    }

    void SetDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        Light light = visibleLight.light;
        m_directionalLightColors[index] = light.color.linear * light.intensity;
        m_directionalLightDirections[index] = -light.transform.forward;
    }

}
