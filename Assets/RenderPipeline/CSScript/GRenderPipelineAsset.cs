using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName="Rendering/Game Render Pipeline")]
public class GRenderPipelineAsset : RenderPipelineAsset
{
    public ShadowSettings m_shadowSetting = default;

    public bool m_enableDynamicBatching = true;
    public bool m_enableGPUInstance = true;
    public bool m_enableSRPBatcher = true;

    protected override RenderPipeline CreatePipeline()
    {
        return new GRenderPipeline(m_enableDynamicBatching, m_enableGPUInstance, m_enableSRPBatcher, m_shadowSetting);
    }
}
