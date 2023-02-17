
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PoolData
{
    //缓存池中可能有多个不同种类的物体，为方便管理故需要设置一个父物体
    public GameObject fatherObj;
    //使用List链式结构来存储物体
    public List<GameObject> poolList;

    //构造函数，进行PoolData的一些初始化
    public PoolData(GameObject obj,GameObject poolObj)
    {
        fatherObj = obj;
        fatherObj.transform.parent = poolObj.transform;
        poolList = new List<GameObject>();
        PushObj(obj);
    }
	//将物体push进缓存池
    public void PushObj(GameObject obj)
    {
        //存储、设置父物体、隐藏
        poolList.Add(obj);
        obj.transform.SetParent(fatherObj.transform);
        obj.SetActive(false);
    }
	//将物体从缓存池取出
    public GameObject GetObj()
    {
        GameObject obj = null;
        obj = poolList[0];
        poolList.RemoveAt(0);
        obj.SetActive(true);
        obj.transform.parent = null;
        return obj;
    }
}

//缓存池定义完成，我们需要一个对缓存池进行管理的单例模式类
public class PoolMgr : BaseManager<PoolMgr>
{
    //使用字典存储数据
    private Dictionary<string, PoolData> poolDic
        = new Dictionary<string, PoolData>();
	//缓存池的父物体
    private GameObject poolObj;
	
    //从缓存池中取出
    public void GetObj(string objName, UnityAction<GameObject> callback)
    {
        //若缓存池中存在，取出
        if (poolDic.ContainsKey(objName) && poolDic[objName].poolList.Count > 0)
            callback(poolDic[objName].GetObj());
        //若不存在则使用ResMgr动态加载
        else
        {
            //注意生成的物体的this.name此时并不是objName，而是objName（clone），若是使用this.name进行PushObj的话，则无法使用objName从字典中进行Get
            ResMgr.GetInstance().LoadAsync<GameObject>(objName, callback);
        }
    }
	//将物体存如缓存池
	//使用时注意objName，使用动态加载出来的物体的this.name会在原名后加上（clone)字样，此时使用this.name进行PushObj操作时，实际是创建了另一个池子，所以使用时推荐直接使用"objName"的方式而不是this.name的方式
    public void PushObj(string objName, GameObject obj)
    {
        if (poolObj == null)
            poolObj = new GameObject("Pool");//实例化，此后所以在缓存池的物体全部为其紫萼u提
        if(poolDic.ContainsKey(objName))//如果缓存池中已经存在其类型，则将物体加入其中
            poolDic[objName].PushObj(obj);
        else//若缓存池中没有此类物体，则添加至字典
            poolDic.Add(objName,new PoolData(obj,poolObj)); 
        //我们采用的就结构是PoolData 类，里面含有链式结构PoolList 
    }
	//清空缓存池
    public void Clear()
    {
        poolDic.Clear();
        poolObj = null;
    }
}
