#ifndef GAME_RENDER_PIPELINE_BRDF
#define GAME_RENDER_PIPELINE_BRDF

struct BrdfData
{
    float3 diffuse;
    float3 specular;
    float  perceptualRoughness;
    float  roughness;
    float  grazingTerm;
    float  roughness2;
};

// https://www.zhihu.com/question/48050245
// 法线分布函数
// D = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2
float GetFactorD(float3 n, float3 h, float r)
{
    float ndoth = saturate(dot(n, h));
    float r2 = r*r;
    float ndoth2 = ndoth*ndoth;
    float denom = ndoth2*(r2 - 1.0f) + 1.00001f;
    return r2/(denom*denom);
}

//几何函数
float GetFactorG(float3 n, float3 l, float3 v, float r)
{
    float k = (r+1)*(r+1)/8.0; // r*r/2.0
    float ndotv = saturate(dot(n, v));
    float ndotl = saturate(dot(n, l));
    float g1 = ndotv/(ndotv*(1-k)+k);
    float g2 = ndotl/(ndotl*(1-k)+k);
    return g1*g2;
}

//菲涅尔系数
float3 GetFactorF(float3 f0, float3 v, float3 h)
{
    float vdoth = saturate(dot(v, h));
    return f0 + (1-f0)*pow(1-vdoth, 5);
}

//获取
float3 GetBrdf(float3 n, float3 l, float3 v, float r, float3 f0)
{
    float3 h = SafeNormalize(l+v);
    float d = GetFactorD(n,h,r)/3.1415926;
    float g = GetFactorG(n,l,v,r);
    float3 f = GetFactorF(f0, v, h);

    float ndotl = saturate(dot(n, l));
    float ndotv = saturate(dot(n, v));
    float denom = 4.0*ndotl*ndotv + 0.00001f;
    return d*g*f/denom;
}

// http://www.thetenthplanet.de/archives/255
float GetFactorVF(float3 l, float3 h, float r)
{
    //V * F = 1.0 / ( LoH^2 * (roughness + 0.5) )
    // See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
    // https://community.arm.com/events/1155
    float ldoth = saturate(dot(l, h));
    float ldoth2 = max(0.1h, ldoth*ldoth);
    float denom = ldoth2*(r + 0.5f);
    return 1.0/denom;
}

float3 GetUnityBrdf(float3 n, float3 l, float3 v, float r, float3 f0)
{
    float3 h = SafeNormalize(l+v);
    return GetFactorD(n,h,r)*GetFactorVF(l,h,r)/4.0f;
}

#endif
