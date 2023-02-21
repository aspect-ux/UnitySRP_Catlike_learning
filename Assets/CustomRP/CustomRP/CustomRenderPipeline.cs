using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    CameraRenderer renderer = new CameraRenderer();

    //����������
    private bool useDynamicBatching, useGPUInstancing;

    ShadowSettings shadowSettings;

    //���캯������ʼ�����ߵ�һЩ����
    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher,ShadowSettings shadowSettings)
    {
        this.shadowSettings = shadowSettings;
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        //����SRP Batch
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
    }
    
    //��дRender,��Ⱦ����ÿ֡����
    protected override void Render(ScriptableRenderContext context,Camera[] cameras)
    {
        foreach(Camera camera in cameras) {
            renderer.Render(context, camera, useDynamicBatching, useGPUInstancing,shadowSettings);
        }
    }
}


