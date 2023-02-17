using System.Collections;
using UnityEngine;
using UnityEngine.Events;


//单例模式继承自BaseManager，使用时可以通过GetInstance调用
public class ResMgr : BaseManager<ResMgr>
{
    //同步加载
    //使用是要注意给出T的类型，如ResMagr.GetInstance().Load<GameObject>()
    public T Load<T>(string objName) where T : Object
    {
        T res = Resources.Load<T>(objName);
        //如果res是一个GameObject
        if (res is GameObject)
        {
            T t = GameObject.Instantiate(res);
            t.name = objName;
            return t;
        }
        else //else情况示例：TextAsset、AudioClip
            return res;
    }
    //异步加载，异步加载使用起来在观感上更加顺滑，适用于较大的资源
    //异步加载使用协程
    public void LoadAsync<T>(string name, UnityAction<T> callback) where T : Object
    {
        //由于是单例模式，要使用协程需要用到MonoMgr
        MonoMgr.GetInstance().StartCoroutine(ReallyLoadAsync<T>(name, callback));
    }
	
    private IEnumerator ReallyLoadAsync<T>(string _name, UnityAction<T> callback) where T : Object
    {
        ResourceRequest r = Resources.LoadAsync<T>(_name);
        yield return r;//直到系统读取完
        if (r.asset is GameObject)
        {
            T t = GameObject.Instantiate(r.asset) as T;
            t.name = _name;//去点(clone)字段
            callback(t);//callback方法作用很大，例如获得其组件脚本，改变其状态        
        }
        else
            r.asset.name = _name;
        callback(r.asset as T);
    }
}