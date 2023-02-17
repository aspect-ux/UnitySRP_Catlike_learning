using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

public class MonoMgr : BaseManager<MonoMgr>
{
    //为了能够使用并管理MonoController,我们首先需要一个MonoController对象
    private MonoController controller;
	
    //由于是单例模式，我们创建构造函数来进行一些必要的初始化
    public MonoMgr()
    {
        //MonoController并不是单例模式，为了使用我们要让它在游戏中生成，首先新创建一个游戏物体名为MonoController
        GameObject obj = new GameObject("MonoController");
        //挂载脚本，并获得此脚本的引用controller
        controller = obj.AddComponent<MonoController>();
    }
    //调用controller里的添加监听的方法，注意传入的参数为无参无返回值的方法
    public void AddUpdateListener(UnityAction func)
    {
        controller.AddUpdateListener(func);
    }
    //同上，不过此方法针对的是随时间更新
    public void AddFixUpdateListener(UnityAction func)
    {
        controller.AddFixedUpdateListener(func);
    }
    //移除监听在Update里的方法
    public void RemoveUpdeteListener(UnityAction func)
    {
        controller.RemoveUpdateListener(func);
    }
    //同上移除监听在FixUpdater的方法
    public void RemoveFixUpdateListener(UnityAction func)
    {
        controller.RemoveFiUpdateListener(func);
    }

    //开启协程方法及其重载方法，使用时注意选择
    public Coroutine StartCoroutine(IEnumerator routine)
    {
        return controller.StartCoroutine(routine);
    }

    public Coroutine StartCoroutine(string methodName, [DefaultValue("null")] object value)
    {
        return controller.StartCoroutine(methodName, value);
    }

    public Coroutine StartCoroutine(string methodName)
    {
        return controller.StartCoroutine(methodName);
    }

}