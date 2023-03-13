using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer 
{
    ScriptableRenderContext context;

    Camera camera;

    CullingResults cullingResults;

    const string bufferName = "Render Camera";

    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
		litShaderTagId = new ShaderTagId("CustomLit");//indicate which pass is allowed

    Lighting lighting = new Lighting();//get a light

    /*???partial class?У??????editor?б?????release apps not need
    static ShaderTagId[] legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };*/

    //static Material errorMaterial;

    public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing, ShadowSettings shadowSettings)
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();//?????scene window ??????buffer???????????????????????????
                        //???????buffer?????????????????????????????buffer??

        PrepareForSceneWindow();//???cull??????UI???????

        if (!Cull(shadowSettings.maxDistance)) return;//Culling

        //Setup();

        //region:include lighting.Setup(...)将阴影包含在mainCamera buffer下
        buffer.BeginSample(SampleName);
        ExecuteBuffer();

        lighting.Setup(context,cullingResults, shadowSettings);//set up light

        buffer.EndSample(SampleName);
        //region

        Setup();

        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);

        DrawUnsupportedShaders();

        DrawGizmos();

        lighting.Cleanup();//clean up before submit

        Submit();

        //SetUp();

        //context.SetUpCameraProperties(camera);
    }

    void Setup()
    {
        //Z + Color + Stencil
        context.SetupCameraProperties(camera);//??????????????????
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
                camera.backgroundColor.linear : Color.clear
        );
        //buffer.ClearRenderTarget(true, true, Color.clear);//??beginSamele????????redundant nesting
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    /*???partial class?У??????editor?б?????release apps not need
    void DrawUnsupportedShaders()
    {
        //error shader
        if (errorMaterial == null)
        {
            errorMaterial =
                new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        
        var drawingSettings = new DrawingSettings(
            legacyShaderTagIds[0], new SortingSettings(camera)
        ){
            overrideMaterial = errorMaterial
        };
        for (int i = 1; i < legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
    }*/

    void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        //var sortingSettings = new SortingSettings(camera);
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque//?????????
        };
        var drawingSettings = new DrawingSettings(
            unlitShaderTagId, sortingSettings
        );
        //configure batching
        drawingSettings.enableDynamicBatching = useDynamicBatching;
        drawingSettings.enableInstancing = useGPUInstancing;

        drawingSettings.SetShaderPassName(1, litShaderTagId);//设置此通道可渲染的着色器通道 CustomLit,index按顺序渲染
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);//eliminate the transparent

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings 
        );

        context.DrawSkybox(camera);

        //start draw transparent
        sortingSettings.criteria = SortingCriteria.CommonTransparent;//???????
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;//change to transparent

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings);
    }

    void Submit()
    {
        buffer.EndSample(SampleName);//sampleName????editor partial????
        ExecuteBuffer();
        context.Submit();
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    bool Cull(float maxShadowDistance)
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }
}
