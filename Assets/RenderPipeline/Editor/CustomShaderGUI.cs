using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomShaderGUI : ShaderGUI {

	MaterialEditor editor;
	Object[] materials;
	MaterialProperty[] properties;

	bool showPresets;

	bool Clipping {
		set => SetProperty("_Clipping", "_CLIPPING", value);
	}

	BlendMode SrcBlend {
		set => SetProperty("_SrcBlend", (float)value);
	}

	BlendMode DstBlend {
		set => SetProperty("_DstBlend", (float)value);
	}

	bool ZWrite {
		set => SetProperty("_ZWrite", value ? 1f : 0f);
	}

	RenderQueue RenderQueue {
		set {
			foreach (Material m in materials) {
				m.renderQueue = (int)value;
			}
		}
	}

	public override void OnGUI (
		MaterialEditor materialEditor, MaterialProperty[] properties
	) {
		base.OnGUI(materialEditor, properties);
		editor = materialEditor;
		materials = materialEditor.targets;
		this.properties = properties;

		EditorGUILayout.Space();
		showPresets = EditorGUILayout.Foldout(showPresets, "Presets", true);
		if (showPresets) {
			OpaquePreset();
			FadePreset();
			ClipPreset();
			TransparentPreset();
		}
	}

	void OpaquePreset () {
		if (PresetButton("Opaque")) {
			Clipping = false;
			SrcBlend = BlendMode.One;
			DstBlend = BlendMode.Zero;
			ZWrite = true;
			RenderQueue = RenderQueue.Geometry;
		}
	}

	void ClipPreset () {
		if (PresetButton("Clip")) {
			Clipping = true;
			SrcBlend = BlendMode.One;
			DstBlend = BlendMode.Zero;
			ZWrite = true;
			RenderQueue = RenderQueue.AlphaTest;
		}
	}

	void FadePreset () {
		if (PresetButton("Fade")) {
			Clipping = false;
			SrcBlend = BlendMode.SrcAlpha;
			DstBlend = BlendMode.OneMinusSrcAlpha;
			ZWrite = false;
			RenderQueue = RenderQueue.Transparent;
		}
	}

	void TransparentPreset () {
		if (PresetButton("Transparent")) {
			Clipping = false;
			SrcBlend = BlendMode.One;
			DstBlend = BlendMode.OneMinusSrcAlpha;
			ZWrite = false;
			RenderQueue = RenderQueue.Transparent;
		}
	}

	bool PresetButton (string name) {
		if (GUILayout.Button(name)) {
			editor.RegisterPropertyChangeUndo(name);
			return true;
		}
		return false;
	}

	bool HasProperty (string name) =>
		FindProperty(name, properties, false) != null;

	void SetProperty (string name, string keyword, bool value) {
		if (SetProperty(name, value ? 1f : 0f)) {
			SetKeyword(keyword, value);
		}
	}

	bool SetProperty (string name, float value) {
		MaterialProperty property = FindProperty(name, properties, false);
		if (property != null) {
			property.floatValue = value;
			return true;
		}
		return false;
	}

	void SetKeyword (string keyword, bool enabled) {
		if (enabled) {
			foreach (Material m in materials) {
				m.EnableKeyword(keyword);
			}
		}
		else {
			foreach (Material m in materials) {
				m.DisableKeyword(keyword);
			}
		}
	}
}