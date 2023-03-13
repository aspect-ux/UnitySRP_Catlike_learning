using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

partial class CameraRenderer 
{
    //声明一下，不然build之后因为找不到函数会报错
    partial void DrawGizmos();

    partial void DrawUnsupportedShaders();

    partial void PrepareForSceneWindow();

    partial void PrepareBuffer();

#if UNITY_EDITOR

    string SampleName { get; set; }

    static ShaderTagId[] legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

    static Material errorMaterial;

    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
    }

    partial void DrawUnsupportedShaders()
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
    }

    partial void PrepareForSceneWindow()
    {
        //使用了Profiler. 来将相机检索包装在Editor Only中，表明只在编辑器中分配内存，在生成时不会分配
        if (camera.cameraType == CameraType.SceneView)
        {
            Profiler.BeginSample("Editor Only");
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            Profiler.EndSample();
        }
    }

    partial void PrepareBuffer()
    {
        buffer.name = SampleName = camera.name;
    }
#else
    string SampleName => bufferName;
#endif
}
