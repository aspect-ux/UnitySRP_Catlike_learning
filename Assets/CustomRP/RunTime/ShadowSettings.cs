using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Purely a container for configuration options
[System.Serializable]
public class ShadowSettings
{
    [Min(0.001f)]
    public float maxDistance = 100f;

    [Range(0.001f, 1f)]
    public float distanceFade = 0.1f;

    //TextureSize enum 选择纹理大小范围从256到8192
    public enum TextureSize
    {
        _256 = 256,_512 = 512,_1024 = 1024,
        _2048 = 2048,_4096 = 4096,_8192 = 8192
    }
    //定义直接光阴影 结构体
    [System.Serializable]
    public struct Directional {
        //设置图集大小
        public TextureSize atlasSize;
        //单个光源阴影级联图数量
        [Range(1, 4)]
        public int cascadeCount;
        //级联比例
        [Range(0f, 1f)]
        public float cascadeRatio1, cascadeRatio2, cascadeRatio3;
        public Vector3 CascadeRatios =>
            new Vector3(cascadeRatio1, cascadeRatio2, cascadeRatio3);
        [Range(0.001f, 1f)]
        public float cascadeFade;

    }

    public Directional directional = new Directional()
    {
        //默认图集的size为1024
        atlasSize = TextureSize._1024,

        cascadeCount = 4,
        cascadeRatio1 = 0.1f,
        cascadeRatio2 = 0.25f,
        cascadeRatio3 = 0.5f,
        cascadeFade = 0.1f
    };
}
