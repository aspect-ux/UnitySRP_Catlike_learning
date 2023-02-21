using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

//works like CameraRender but for light
public class Lighting
{
	
	const string bufferName = "Lighting";

	//������Դ����
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

		//ѭ����������Vector����
		int dirLightCount = 0;
		for (int i = 0; i < visibleLights.Length; i++)
		{
			VisibleLight visibleLight = visibleLights[i];

			//ֻ���÷����Դ
			if (visibleLight.lightType == LightType.Directional)
			{
				//���������е�����Դ������
				SetupDirectionalLight(dirLightCount++, ref visibleLight);
				if (dirLightCount >= maxDirLightCount)
				{
					//��󲻳���4�������Դ
					break;
				}
			}
		}

		//���ݵ�ǰ��Ч��Դ������Դ��ɫVector���顢��Դ����Vector���顣
		//��������ʵ�ʴ��ݸ�GPU���Ǽ�άVector���䴫�ݵ�Vector�Ǻ�ΪVector4��
		buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
		buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
		buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
		buffer.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
	}

	void SetupDirectionalLight(int index, ref VisibleLight visibleLight) 
	{
		//VisibleLight.finalColorΪ��Դ��ɫ��ʵ���ǹ�Դ��ɫ*��Դǿ�ȣ�����Ĭ�ϲ���������ɫ�ռ䣬��Ҫ��Graphics.lightsUseLinearIntensity����Ϊtrue��
		dirLightColors[index] = visibleLight.finalColor;
		//��Դ����Ϊ��ԴlocalToWorldMatrix�ĵ����У�����Ҳ��Ҫȡ��
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