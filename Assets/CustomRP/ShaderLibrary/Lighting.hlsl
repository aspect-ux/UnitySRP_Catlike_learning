#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

float3 IncomingLight(Surface surface, Light light) {// caculate how much incoming light there is
	return saturate(dot(surface.normal, light.direction)) * light.color;
}

float3 GetLighting(Surface surface, BRDF brdf, Light light) {
	return IncomingLight(surface, light) * DirectBRDF(surface, brdf, light);
}


float3 GetLighting(Surface surface,BRDF brdf) {
	//return surface.normal.y * surface.color;
	//return GetLighting(surface, GetDirectionalLight());
	//使用循环，累积所有有效方向光源的光照计算结果
	float3 color = 0.0;
	for (int i = 0; i < GetDirectionalLightCount(); i++)
	{
		color += GetLighting(surface,brdf,GetDirectionalLight(i));
	}
	return color;
}


#endif