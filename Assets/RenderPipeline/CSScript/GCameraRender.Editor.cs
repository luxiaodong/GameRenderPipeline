using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

partial class GCameraRender
{
    partial void DrawUnsupportedShaders();
    partial void DrawGizmos();
    partial void PrepareBuffer();

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

    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos()) {
			m_context.DrawGizmos(m_camera, GizmoSubset.PreImageEffects);
			m_context.DrawGizmos(m_camera, GizmoSubset.PostImageEffects);
		}
    }

    partial void PrepareBuffer()
    {
        // Profiler.BeginSample("Editor Only");
        m_buffer.name = m_camera.name;
        m_sampleName = m_camera.name;
        // Profiler.EndSample();
    }

#endif

}
