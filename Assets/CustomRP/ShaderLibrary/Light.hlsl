#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

//使用宏定义最大方向光源数，需要与cpu端匹配
#define MAX_DIRECTIONAL_LIGHT_COUNT 4

//To make light's data accessible and uniform for shader properties
//cpu会每帧传递（修改）这两个属性到GPU的常量缓冲区
CBUFFER_START(_CustomLight) 
/*float3 _DirectionalLightColor;
float3 _DirectionalLightDirection;*/
//当前有效光源数
int _DirectionalLightCount;
//注意CBUFFER中创建数组的格式,在Shader中数组在创建时必须明确其长度，创建完毕后不允许修改
float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light {
	float3 color;
	float3 direction;
};

int GetDirectionalLightCount()
{
	return _DirectionalLightCount;
}

Light GetDirectionalLight(int index) {
	Light light;
	/*light.color = _DirectionalLightColor;
	light.direction = _DirectionalLightDirection;*/
	light.color = _DirectionalLightColors[index].rgb;
	light.direction = _DirectionalLightDirections[index].xyz;
	return light;
}

#endif