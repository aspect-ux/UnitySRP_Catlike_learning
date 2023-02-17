//定义BRDF函数需要传入的参数
#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

//宏定义最小高光反射率
#define MIN_REFLECTIVITY 0.04

float OneMinusReflectivity(float metallic)
{
    //将漫反射反射率控制在[0,0.96]内
    float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

struct BRDF
{
    //物体表面漫反射颜色
    float3 diffuse;
    //物体表面高光颜色
    float3 specular;
    //物体表面粗糙度
    float3 roughness;
};

BRDF GetBRDF(Surface surface)
{
    BRDF brdf;

    //Reflectivity表示Specular反射率
    float oneMinusReflectivity = 1.0 - surface.metallic;
    //diffuse等于物体表面不吸收的光能量color*（1-高光反射率）
    brdf.diffuse = surface.color * oneMinusReflectivity;
    //暂时使用固定值
    //brdf.specular = 0.0;
    //brdf.specular = surface.color - brdf.diffuse;
    brdf.roughness = 1.0;
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
    return brdf;
}

float SpecularStrength(Surface surface, BRDF brdf, Light light) {
    float3 h = SafeNormalize(light.direction + surface.viewDirection);
    float nh2 = Square(saturate(dot(surface.normal, h)));
    float lh2 = Square(saturate(dot(light.direction, h)));
    float r2 = Square(brdf.roughness);
    float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
    float normalization = brdf.roughness * 4.0 + 2.0;
    return r2 / (d2 * max(0.1, lh2) * normalization);
}

float3 DirectBRDF(Surface surface, BRDF brdf, Light light) {
    return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}
#endif