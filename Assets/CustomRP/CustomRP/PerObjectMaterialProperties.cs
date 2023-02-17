
using UnityEngine;

[DisallowMultipleComponent]//不允许多次挂该组件
public class PerObjectMaterialProperties : MonoBehaviour
{
    private static int baseColorId = Shader.PropertyToID("_BaseColor");//映射到属性，不代表属性本身
    static int cutoffId = Shader.PropertyToID("_Cutoff"),
    metallicId = Shader.PropertyToID("_Metallic"),
		smoothnessId = Shader.PropertyToID("_Smoothness");

    [SerializeField] Color baseColor = Color.white;

    private static MaterialPropertyBlock block;

    [SerializeField, Range(0f, 1f)]
    float cutoff = 0.5f,metallic = 0f, smoothness = 0.5f;

    private void OnValidate()
    {
        if (block == null) {
            block = new MaterialPropertyBlock();
        }

        //设置block中的baseColor属性(通过baseCalorId索引)为baseColor
        block.SetColor(baseColorId, baseColor);
        block.SetFloat(cutoffId, cutoff);

        block.SetFloat(metallicId, metallic);
        block.SetFloat(smoothnessId, smoothness);
        //将物体的Renderer中的颜色设置为block中的颜色
        GetComponent<Renderer>().SetPropertyBlock(block);
    }
    //Runtime时也执行
    private void Awake()
    {
        OnValidate();
    }

}
