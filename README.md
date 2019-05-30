
### XSession 是什么

CXSession 是一个解决现有老项目中Session性能问题的工具类库。

<p><br /><br /></p>

### Session 对性能的影响
 - 一个用户的多个并发请求，在服务端以串行的方式执行，导致Session阻塞。
 - Session数据的加载耗时：请求外部进程 -> 读取Session数据 -> TCP传输 -> 反序列化 -> 还原Session结构
 - Session数据的保存耗时：Session结构序列化 -> TCP传输 -> 外部存储进程 -> 持久化 
 
Session对性能有较大影响的就是前二类场景。
<br /><br />
此外，一个用户的Session数据视为一个整体做存储和加载，即使请求中只使用一个key/value，也会导致全部加载，更新时亦然。当Session数据中存放“大对象”时将会影响性能。
  
<p><br /><br /><br /></p>

### Session 的使用建议

<p><b style="color: red; font-size: 24px">如果没有特殊的原因，建议不要使用 Session !</b></p>
<p><br /></p>
可以考虑使用 Cookie + Cache 的方式来代替。


```
// 例如：
var xx = httpContext.Session["someKey"];

// 等同于：
string key = GetCookie("userId") + "someKey";
var xx = SomeCache.Get(key);
```
说明：这里的Cache是指一些外部的缓存组件，诸如 Memcache, Redis 之类。
  
<p><br /><br /><br /></p>


### XSession 做了什么
XSession提供了二个 “Session存储” 的实现类：
 - FileSessionStateStore: 采用文件存储Session数据
 - FastSessionStateStore: 采用文件和Cache存储Session数据
 
FileSessionStateStore特点：
 1. 参考PHP之类的做法，将Session数据保存在文件中，避免程序重启导致Session数据丢失
 2. 使用文件存储可以减少网络调用开销，提升性能。
 3. 注意事项：需要在负载均衡上设置【<b>用户会话关联</b>】


 FastSessionStateStore主要优化点在于：
 1. 内部包含FileSessionStateStore的所有功能，用文件做Session数据的持久化
 2. 进一步提升Sessio加载性能：Session数据写入文件时，同时存放一份在内存中，加载时优先从内存中获取
 2. 避免32位内存不够用时导致OOM，“内存优先”功能仅在64位运行时时有效
 
<p><br /><br /></p>

### XSession 使用方法
在 web.config 中配置 customProvider

```
<system.web>

    <sessionState mode="Custom" customProvider="FastSessionStateStore" cookieless="false" timeout="30" >
        <providers>
            <add name="FileSessionStateStore" type="XSession.Modules.FileSessionStateStore, XSession.Modules"/>
            <add name="FastSessionStateStore" type="XSession.Modules.FastSessionStateStore, XSession.Modules"/>
        </providers>
    </sessionState>

    <httpModules>
        <add name="SessionDetectionModule" type="XSession.Modules.Debug.SessionDetectionModule, XSession.Modules"/>
    </httpModules>

</system.web>
```
使用建议：
 - 可直接使用 FastSessionStateStore
 - 如果内存不够大，可使用 FileSessionStateStore
 - SessionDetectionModule 用于提供一些诊断信息，可自行决定要不要使用。

<p><br /><br /></p>
 
### 源代码项目说明
 - SessionCodeAnalysis：代码分析工具，扫描Session的使用方法
 - WebApplication1：测试站点
 - XSession.Modules：解决Session性能问题的工具类库项目
 
 

