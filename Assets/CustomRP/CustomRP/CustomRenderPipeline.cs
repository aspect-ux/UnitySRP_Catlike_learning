using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    CameraRenderer renderer = new CameraRenderer();

    //批处理配置
    private bool useDynamicBatching, useGPUInstancing;

    //构造函数，初始化管线的一些属性
    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher)
    {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        //配置SRP Batch
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
    }
    
    //重写Render,渲染管线每帧调用
    protected override void Render(ScriptableRenderContext context,Camera[] cameras)
    {
        foreach(Camera camera in cameras) {
            renderer.Render(context, camera, useDynamicBatching, useGPUInstancing);
        }
    }
}


