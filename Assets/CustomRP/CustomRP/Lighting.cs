using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

//works like CameraRender but for light
public class Lighting
{
	
	const string bufferName = "Lighting";

	//最大方向光源数量
	private const int maxDirLightCount = 4;

	CommandBuffer buffer = new CommandBuffer
	{
		name = bufferName
	};

	//keep track of the identifier of the shader properties
	static int
		dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
		dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections"),
		dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
		dirLightShadowDataId =
				Shader.PropertyToID("_DirectionalLightShadowData");

	private static Vector4[] dirLightColors = new Vector4[maxDirLightCount],
		dirLightDirections = new Vector4[maxDirLightCount],
		dirLightShadowData = new Vector4[maxDirLightCount];

	CullingResults cullingResults;

	Shadows shadows = new Shadows();//keep track of shadow instance


	public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
	{
		this.cullingResults = cullingResults;
		buffer.BeginSample(bufferName);
		shadows.Setup(context, cullingResults, shadowSettings);
		//SetupDirectionalLight(); replaced with SetupLights()
		SetupLights();
		shadows.Render();
		buffer.EndSample(bufferName);
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

	void SetupLights()
    {
		//native memory buffer,make it possible to fastly share data between c# and unity engine code
		NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;

		//循环配置两个Vector数组
		int dirLightCount = 0;
		for (int i = 0; i < visibleLights.Length; i++)
		{
			VisibleLight visibleLight = visibleLights[i];

			//只配置方向光源
			if (visibleLight.lightType == LightType.Directional)
			{
				//设置数组中单个光源的属性
				SetupDirectionalLight(dirLightCount++, ref visibleLight);
				if (dirLightCount >= maxDirLightCount)
				{
					//最大不超过4个方向光源
					break;
				}
			}
		}

		//传递当前有效光源数、光源颜色Vector数组、光源方向Vector数组。
		//不管我们实际传递给GPU的是几维Vector，其传递的Vector是恒为Vector4的
		buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
		buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
		buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
		buffer.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
	}

	void SetupDirectionalLight(int index, ref VisibleLight visibleLight) 
	{
		//VisibleLight.finalColor为光源颜色（实际是光源颜色*光源强度，但是默认不是线性颜色空间，需要将Graphics.lightsUseLinearIntensity设置为true）
		dirLightColors[index] = visibleLight.finalColor;
		//光源方向为光源localToWorldMatrix的第三列，这里也需要取反
		dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
		//reserve shadows
		dirLightShadowData[index] = shadows.ReserveDirectionalShadows(visibleLight.light, index);
		//access the scene's main light
		//explicitly configured via Window / Rendering / Lighting Settings
		//Use CommandBuffer.SetGlobalVector to send the light data to the GPU
		/*Light light = RenderSettings.sun;
		buffer.SetGlobalVector(dirLightColorsId, light.color.linear * light.intensity);
		buffer.SetGlobalVector(dirLightDirectionsId, -light.transform.forward);*/
	}

	public void Cleanup()
	{
		shadows.Cleanup();
	}
}