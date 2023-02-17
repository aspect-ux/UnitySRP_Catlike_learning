//����BRDF������Ҫ����Ĳ���
#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

//�궨����С�߹ⷴ����
#define MIN_REFLECTIVITY 0.04

float OneMinusReflectivity(float metallic)
{
    //�������䷴���ʿ�����[0,0.96]��
    float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

struct BRDF
{
    //���������������ɫ
    float3 diffuse;
    //�������߹���ɫ
    float3 specular;
    //�������ֲڶ�
    float3 roughness;
};

BRDF GetBRDF(Surface surface)
{
    BRDF brdf;

    //Reflectivity��ʾSpecular������
    float oneMinusReflectivity = 1.0 - surface.metallic;
    //diffuse����������治���յĹ�����color*��1-�߹ⷴ���ʣ�
    brdf.diffuse = surface.color * oneMinusReflectivity;
    //��ʱʹ�ù̶�ֵ
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