//����������Ӱ��ͼ
#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

//�궨�����֧����Ӱ�ķ����Դ����Ҫ��CPU��ͬ����Ϊ4
#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

//����CPU�˴�����ShadowAtlas
//ʹ��TEXTURE2D_SHADOW����ȷ���ǽ��յ�����Ӱ��ͼ
TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
//��Ӱ��ͼֻ��һ�ֲ�����ʽ�����������ʽ����һ����Ӱ������״̬������Ҫ�����κ�������������Ϊsampler_linear_clamp_compare(ʹ�ú궨����ΪSHADOW_SAMPLER)
//�ɴˣ������κ���Ӱ��ͼ�����Ƕ�����ʹ��SHADOW_SAMPLER���������״̬
//sampler_linear_clamp_compare���ȡ��ʮ���н�����Unity�Ὣ������ַ����ʹ��Linear���ˡ�Clamp������������ȱȽϵĲ�����
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

//����CPU�˴�����ÿ��Shadow Tile����Ӱ�任����
CBUFFER_START(_CustonShadows)
float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
int _CascadeCount;
float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
float4 _CascadeData[MAX_CASCADE_COUNT];
float4 _ShadowDistanceFade;
CBUFFER_END


//ÿ�������Դ�ĵ���Ӱ��Ϣ��������֧����Ӱ�Ĺ�Դ����֧�֣�����Ӱǿ�Ⱦ���0��
struct DirectionalShadowData
{
    float strength;
    int tileIndex;
    float normalBias;
};

struct ShadowData {
    int cascadeIndex;
    float strength;
};

float FadedShadowStrength(float distance, float scale, float fade) {
    return saturate((1.0 - distance * scale) * fade);
}

ShadowData GetShadowData(Surface surfaceWS) {
    ShadowData data;
    /*data.strength = FadedShadowStrength(
        surfaceWS.depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y
    );*/
    //data.strength = 1.0;
    int i;
    //If we end up beyond the last cascade them there is most likely no valid shadow data and we should not sample shadows at all
    for (i = 0; i < _CascadeCount; i++) {
        float4 sphere = _CascadeCullingSpheres[i];
        float distanceSqr = DistanceSquared(surfaceWS.position, sphere.xyz);
        if (distanceSqr < sphere.w) {
            if (i == _CascadeCount - 1) {
                data.strength *= FadedShadowStrength(
                    distanceSqr, _CascadeData[i].x, _ShadowDistanceFade.z
                );
            }
            break;
        }
    }

    if (i == _CascadeCount) {
        data.strength = 0.0;
    }

    data.cascadeIndex = i;
    return data;
}

//����ShadowAtlas������positionSTS��STS��Shadow Tile Space������Ӱ��ͼ��ӦTile���ؿռ��µ�ƬԪ���꣩
float SampleDirectionalShadowAtlas(float3 positionSTS)
{
    //ʹ���ض�����������Ӱ��ͼ
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

//������Ӱ˥��ֵ������ֵ[0,1]��0������Ӱ˥�����ƬԪ��ȫ����Ӱ�У���1������Ӱ˥�����٣�ƬԪ��ȫ�������䡣��[0,1]���м�ֵ����ƬԪ��һ��������Ӱ��
/*
float GetDirectionalShadowAttenuation(DirectionalShadowData data, Surface surfaceWS)
{
    //���Բ�������Ӱ����Ӱǿ��Ϊ0�Ĺ�Դ
    if (data.strength <= 0.0)
    {
        return 1.0;
    }
    //���ݶ�ӦTile��Ӱ�任�����ƬԪ�������������Tile�ϵ���������STS
    float3 positionSTS = mul(_DirectionalShadowMatrices[data.tileIndex], float4(surfaceWS.position, 1.0)).xyz;
    //����Tile�õ���Ӱǿ��ֵ
    float shadow = SampleDirectionalShadowAtlas(positionSTS);
    //���ǹ�Դ����Ӱǿ�ȣ�strengthΪ0����Ȼû����Ӱ
    return lerp(1.0, shadow, data.strength);
}*/
float GetDirectionalShadowAttenuation(
    DirectionalShadowData directional, ShadowData global, Surface surfaceWS
) {
    if (directional.strength <= 0.0) {
        return 1.0;
    }
    //float3 normalBias = surfaceWS.normal * _CascadeData[global.cascadeIndex].y;
    float3 normalBias = surfaceWS.normal *
        (directional.normalBias * _CascadeData[global.cascadeIndex].y);
    float3 positionSTS = mul(
        _DirectionalShadowMatrices[directional.tileIndex],
        float4(surfaceWS.position + normalBias, 1.0)
    ).xyz;
    float shadow = SampleDirectionalShadowAtlas(positionSTS);
    return lerp(1.0, shadow, directional.strength);
}

#endif