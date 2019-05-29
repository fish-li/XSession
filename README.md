
### XSession 是什么

CXSession 是一个解决现有老项目中Session性能问题的工具类库。

<p><br /><br /></p>

### Session 对性能的影响
 - 同一用户的多个并发请求导致的Session阻塞
 - Session数据的加载耗时：ASP.NET请求外部存储进程 -> 获取数据 -> TCP传输 -> 反序列化 -> 还原Session结构
 - Session数据的保存耗时：Session结构序列化 -> TCP传输 -> 外部存储进程 -> 持久化 
 
Session对性能有较大影响的就前二类场景。
  
<p><br /><br /><br /></p>

### Session 的使用建议

如果没有特殊的原因，建议不使用 Session ！  只有好处没有坏处！
  
<p><br /><br /><br /></p>


### XSession 做了什么
XSession提供了二个 SessionStateStoreProviderBase 的实现类：
 - FileSessionStateStore: 采用文件存储Session数据的StoreProvider
 - FastSessionStateStore: 采用文件和Cache存储Session数据的StoreProvider
 
FileSessionStateStore特点：
 1. 参考PHP之类的做法，将Session数据保存在文件中，避免程序重启导致Session数据丢失
 2. 使用文件存储可以减少网络调用开销，提升性能。
 3. 注意事项：需要在负载均衡上设置【用户会话关联】


 FastSessionStateStore主要优化点在于：
 1. 内部包含FileSessionStateStore的所有功能，用文件做Session数据的持久化
 2. 提升Sessio加载性能，Session数据在写入文件时，同时存放一份在Cache中，加载时优先从Cache中获取
 2. 避免32位内存不够用时导致OOM，Cache功能仅当64位时有效
 
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
 - SessionDetectionModule 用于提供一些诊断信息，可自行决定要不要使用。

<p><br /><br /></p>
 
### 源代码项目说明
 - SessionCodeAnalysis：代码分析工具，扫描Session的使用方法
 - WebApplication1：测试站点
 - XSession.Modules：解决Session性能问题的工具类库项目
 
 

