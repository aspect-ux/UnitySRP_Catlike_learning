#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

//ʹ�ú궨��������Դ������Ҫ��cpu��ƥ��
#define MAX_DIRECTIONAL_LIGHT_COUNT 4

//To make light's data accessible and uniform for shader properties
//cpu��ÿ֡���ݣ��޸ģ����������Ե�GPU�ĳ���������
CBUFFER_START(_CustomLight) 
/*float3 _DirectionalLightColor;
float3 _DirectionalLightDirection;*/
//��ǰ��Ч��Դ��
int _DirectionalLightCount;
//ע��CBUFFER�д�������ĸ�ʽ,��Shader�������ڴ���ʱ������ȷ�䳤�ȣ�������Ϻ������޸�
float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light {
	float attenuation;
	float3 color;
	float3 direction;
};

int GetDirectionalLightCount()
{
	return _DirectionalLightCount;
}

DirectionalShadowData GetDirectionalShadowData(int lightIndex, ShadowData shadowData) {
	DirectionalShadowData data;
	data.strength =
		_DirectionalLightShadowData[lightIndex].x * shadowData.strength;
	data.tileIndex = _DirectionalLightShadowData[lightIndex].y + shadowData.cascadeIndex;
	data.normalBias = _DirectionalLightShadowData[lightIndex].z;
	return data;
}

Light GetDirectionalLight(int index, Surface surfaceWS, ShadowData shadowData) {
	Light light;
	light.color = _DirectionalLightColors[index].rgb;
	light.direction = _DirectionalLightDirections[index].xyz;
	DirectionalShadowData dirShadowData =
		GetDirectionalShadowData(index, shadowData);
	//light.attenuation = GetDirectionalShadowAttenuation(dirShadowData, surfaceWS);
	//light.attenuation = shadowData.cascadeIndex * 0.25;
	light.attenuation =
		GetDirectionalShadowAttenuation(dirShadowData, shadowData, surfaceWS);
	return light;
}



#endif