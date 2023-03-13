using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{

	const string bufferName = "Shadows";

	CommandBuffer buffer = new CommandBuffer
	{
		name = bufferName
	};

	ScriptableRenderContext context;

	CullingResults cullingResults;

	ShadowSettings settings;

	//֧����Ӱ�Ĺ������ֻ��4��,ÿ�����յļ���ͼ���Ҳֻ��4��
	const int maxShadowedDirectionalLightCount = 4,maxCascades = 4;

	//����Ӱpcf����ģʽShader�ؼ���
	private static string[] directionalFilterKeywords =
	{
		"_DIRECTIONAL_PCF3",
		"_DIRECTIONAL_PCF5",
		"_DIRECTIONAL_PCF7"
	};

	struct ShadowedDirectionalLight
	{
		public int visibleLightIndex;
		public float slopeScaleBias;
	}

	ShadowedDirectionalLight[] ShadowedDirectionalLights =
		new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

	int ShadowedDirectionalLightCount;

	static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
		dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
		cascadeCountId = Shader.PropertyToID("_CascadeCount"),
		cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres"),
		cascadeDataId = Shader.PropertyToID("_CascadeData"),
		shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");

	static Vector4[] cascadeCullingSpheres = new Vector4[maxCascades], cascadeData = new Vector4[maxCascades];

	static Matrix4x4[]
		dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount * maxCascades];

	public void Setup(ScriptableRenderContext context, CullingResults cullingResults,ShadowSettings settings)
	{
		this.context = context;
		this.cullingResults = cullingResults;
		this.settings = settings;

		ShadowedDirectionalLightCount = 0;
	}

	public Vector3 ReserveDirectionalShadows(Light light, int visibleLightIndex)
	{
		if (ShadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
			light.shadows != LightShadows.None && light.shadowStrength > 0f &&
			cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
		{
			ShadowedDirectionalLights[ShadowedDirectionalLightCount] =
				new ShadowedDirectionalLight
				{
					visibleLightIndex = visibleLightIndex,
					slopeScaleBias = light.shadowBias
				};

			return new Vector3(
				light.shadowStrength, settings.directional.cascadeCount * ShadowedDirectionalLightCount++, light.shadowNormalBias
			);
		}
		return Vector3.zero;
	}

	public void Render()
	{
		if (ShadowedDirectionalLightCount > 0)
		{
			RenderDirectionalShadows();
		}
		else// An alternative approach is to instead get a 1��1 dummy texture when no shadows are needed, avoiding extra shader variants
		{
			buffer.GetTemporaryRT(
				dirShadowAtlasId, 1, 1,
				32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
			);
		}
	}

	//Creating a shadow map is done by shadow-casting objects to texture
	void RenderDirectionalShadows()
	{
		int atlasSize = (int)settings.directional.atlasSize;
		buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
		//nstruct the GPU to render to this texture instead of the camera's target.
		buffer.SetRenderTarget(
			dirShadowAtlasId,
			RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store
		);
		buffer.ClearRenderTarget(true, false, Color.clear);

		buffer.BeginSample(bufferName);
		ExecuteBuffer();

		//һ������Ӱͼ����
		int tiles = ShadowedDirectionalLightCount * settings.directional.cascadeCount;
		int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
		//int split = ShadowedDirectionalLightCount <= 1 ? 1 : 2;
		int tileSize = atlasSize / split;

		for (int i = 0; i < ShadowedDirectionalLightCount; i++)
		{
			RenderDirectionalShadows(i, split, tileSize);
		}

		//send the matrices to the GPU
		buffer.SetGlobalInt(cascadeCountId, settings.directional.cascadeCount);
		buffer.SetGlobalVectorArray(
			cascadeCullingSpheresId, cascadeCullingSpheres
		);
		buffer.SetGlobalVectorArray(cascadeDataId, cascadeData);
		buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
		float f = 1f - settings.directional.cascadeFade;
		buffer.SetGlobalVector(
			shadowDistanceFadeId, new Vector4(
				1f / settings.maxDistance, 1f / settings.distanceFade,
				1f / (1f - f * f)
			)
		);
		buffer.EndSample(bufferName);
		ExecuteBuffer();
	}

	void RenderDirectionalShadows(int index, int split, int tileSize)
	{
		ShadowedDirectionalLight light = ShadowedDirectionalLights[index];
		var shadowSettings =
			new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
		int cascadeCount = settings.directional.cascadeCount;
		int tileOffset = index * cascadeCount;
		Vector3 ratios = settings.directional.CascadeRatios;

		for (int i = 0; i < cascadeCount; i++)
		{
			//���㷽������ͼ��ͶӰ�����Լ���Ӱ�ָ�����
			cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
				light.visibleLightIndex, i, cascadeCount, ratios, tileSize, 0f,
				out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
				out ShadowSplitData splitData
			);

			//ShadowDrawingSettings��������ʹ������splitData��Ⱦ�ĸ���Ӱ��Դ��splitDataΪ������Ӱ�����ȼ����޳���Ϣ��Ŀǰ���ǻ�����Ҫ����splitData��
			shadowSettings.splitData = splitData;

			if (index == 0)
			{
				SetCascadeData(i, splitData.cullingSphere, tileSize);
			}

			int tileIndex = tileOffset + i;
			dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(
				projectionMatrix * viewMatrix,
				SetTileViewport(tileIndex, split, tileSize), split
			);
			buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
			buffer.SetGlobalDepthBias(500000f, 0f);
			//buffer.SetGlobalDepthBias(0f, light.slopeScaleBias);
			ExecuteBuffer();
			context.DrawShadows(ref shadowSettings);
			buffer.SetGlobalDepthBias(0f, 0f);
		}
	}

	void SetCascadeData(int index, Vector4 cullingSphere, float tileSize)
	{
		float texelSize = 2f * cullingSphere.w / tileSize;
		cascadeData[index].x = 1f / cullingSphere.w;
		cullingSphere.w *= cullingSphere.w;
		//cascadeCullingSpheres[index] = cullingSphere;
		cascadeData[index] = new Vector4(
			1f / cullingSphere.w,
			texelSize * 1.4142136f);
	}
	/*
	void RenderDirectionalShadows(int index, int split, int tileSize)
	{
		ShadowedDirectionalLight light = ShadowedDirectionalLights[index];
		var shadowSettings =
			new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);

		
		cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
			light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f,
			out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
			out ShadowSplitData splitData
		);
		
		shadowSettings.splitData = splitData;

		dirShadowMatrices[index] =
		ConvertToAtlasMatrix(projectionMatrix * viewMatrix, SetTileViewport(index, split, tileSize), split);
		buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

		ExecuteBuffer();
		context.DrawShadows(ref shadowSettings);//draw shadow texture
	}*/

	public void Cleanup()
	{
		buffer.ReleaseTemporaryRT(dirShadowAtlasId);
		ExecuteBuffer();
	}

	void ExecuteBuffer()
	{
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

	Vector2 SetTileViewport(int index, int split, float tileSize)
	{
		Vector2 offset = new Vector2(index % split, index / split);
		buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
		return offset;
	}

	Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
	{
		//���ʹ�÷���Z��������ΪZȡ��
		if (SystemInfo.usesReversedZBuffer)
		{
			m.m20 = -m.m20;
			m.m21 = -m.m21;
			m.m22 = -m.m22;
			m.m23 = -m.m23;
		}
		//��Դ�ü��ռ����귶ΧΪ[-1,1]���������������ȶ���[0,1]����ˣ����ǽ��ü��ռ�����ת����[0,1]��
		//Ȼ��[0,1]�µ�x,yƫ�Ƶ���Դ��Ӧ��Tile��
		float scale = 1f / split;
		m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
		m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
		m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
		m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
		m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
		m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
		m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
		m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
		m.m20 = 0.5f * (m.m20 + m.m30);
		m.m21 = 0.5f * (m.m21 + m.m31);
		m.m22 = 0.5f * (m.m22 + m.m32);
		m.m23 = 0.5f * (m.m23 + m.m33);
		return m;
	}
}