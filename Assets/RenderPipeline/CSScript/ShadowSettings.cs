using UnityEngine;

[System.Serializable]
public class ShadowSettings {

	public enum ShadowMapSize {
		_512 = 512, _1024 = 1024, _2048 = 2048
	}

	public enum FilterMode {
		PCF_NONE, PCF3x3, PCF5x5, PCF7x7
	}

	[Min(0.001f)]
	public float m_maxDistance = 100f;

	[Range(0.001f, 1f)]
	public float m_fadePercent = 0.1f;

	[System.Serializable]
	public struct Directional {

		public ShadowMapSize m_shadowMapSize;

		public FilterMode m_pcfFilter;

		[Range(1, 4)]
		public int m_cascadeCount;

		[Range(0f, 1f)]
		public float m_cascadeRatio1, m_cascadeRatio2, m_cascadeRatio3;

		// public Vector3 CascadeRatios =>
			// new Vector3(cascadeRatio1, cascadeRatio2, cascadeRatio3);

		// [Range(0.001f, 1f)]
		// public float cascadeFade;

		// public enum CascadeBlendMode {
		// 	Hard, Soft, Dither
		// }

		// public CascadeBlendMode cascadeBlend;
	}

	public Directional m_directional = new Directional {
		m_shadowMapSize = ShadowMapSize._1024,
		m_pcfFilter = FilterMode.PCF_NONE,
		m_cascadeCount = 4,
		m_cascadeRatio1 = 0.1f,
		m_cascadeRatio2 = 0.25f,
		m_cascadeRatio3 = 0.5f,
		// cascadeFade = 0.1f,
		// cascadeBlend = Directional.CascadeBlendMode.Hard
	};
}