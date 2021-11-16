using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName="Rendering/Game Render Pipeline")]
public class GRenderPipelineAsset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new GRenderPipeline();
    }
}
