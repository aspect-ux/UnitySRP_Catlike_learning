# Game

## Event System

方法1：

两个文件

**用法**

* 使用`interface IEventListener`注册事件，卸载事件
* 注册具体方法使用`AddEventListener`,卸载使用`RemoveEventListener`

**综述**

一件事情会有可能会有许多反馈，每当我们要检测一个事件，首先注册这个事件，不断+=添加方法（反馈）,这些都在start(awake)之类中进行，等到游戏结束再取消订阅。

* 包含一个`static EventManager instance;`单一实例并用于外部访问成员数据和方法
* 包含一个`interface IEventInfo`用于外部多态地实现方法
* 包含一个字典`Dictionary<string,IEventInfo> actionDic` 用于存储事件，可以理解成"事件池"
* 包含类`EventInfo<T>`继承自上述接口，用于添加单参数方法
* 包含添加、移除、触发事件

