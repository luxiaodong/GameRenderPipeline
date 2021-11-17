using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

partial class GCameraRender
{
    partial void DrawUnsupportedShaders();

#if UNITY_EDITOR
    static ShaderTagId[] m_legacyShaderTagIds = {
        new ShaderTagId("Always"),
		new ShaderTagId("ForwardBase"),
		new ShaderTagId("PrepassBase"),
		new ShaderTagId("Vertex"),
		new ShaderTagId("VertexLMRGBM"),
		new ShaderTagId("VertexLM")
    };
    static Material m_errorMaterial;

    partial void DrawUnsupportedShaders()
    {
        if(m_errorMaterial == null)
        {
            m_errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        var drawingSettings = new DrawingSettings(m_legacyShaderTagIds[0], new SortingSettings(m_camera))
        {
            overrideMaterial = m_errorMaterial
        };

        for(int i=1; i<m_legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, m_legacyShaderTagIds[i]);
        }
		var filteringSettings = FilteringSettings.defaultValue;
		m_context.DrawRenderers(m_cullingResult, ref drawingSettings, ref filteringSettings);
    }
#endif

}
