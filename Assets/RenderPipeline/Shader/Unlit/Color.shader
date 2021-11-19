Shader "GameRenderPipeline/Unlit/Color"
{
    Properties
    {
        _MainColor ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "Unlit Color"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../Library/Common.hlsl"

            float4 _MainColor;
            
            float4 vert(float3 positionOS : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(positionOS);
            }

            float4 frag() : SV_TARGET
            {
                return _MainColor;
            }

            ENDHLSL
        }
    }
}
