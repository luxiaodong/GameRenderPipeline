Shader "GameRenderPipeline/Unlit/Transparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        
        Pass
        {
            Name "Unlit Transparent"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../Library/Common.hlsl"
            
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _BaseMap_ST;

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(a2v i)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(i.positionOS);
                o.uv = i.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                return o;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
            }

            ENDHLSL
        }
    }
}


