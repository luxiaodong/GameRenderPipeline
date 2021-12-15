Shader "GameRenderPipeline/Test/Normal"
{
    Properties
    {
    }

    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            v2f vert(a2v i)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(i.positionOS);
                o.normalWS = TransformObjectToWorldNormal(i.normalOS);
                return o;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                float3 color = normalize(i.normalWS);
                return float4(color, 1.0f);
            }

            ENDHLSL
        }
    }
}
