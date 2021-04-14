## 介绍
AB作为之前Unity主推的资源管理工作流，可以把模型、贴图、预制体、声音、甚至整个场景都打入压缩包中，然后在游戏过程中再加载。使用他的主要目的有以下几点：1.统一的资源管理、2.做分包、3.热更资源。他也有一个最大的缺点就是需要开发者自己写一套资源管理系统来管理依赖、引用关系。
可寻址资源系统是现在Unity现在主推的资源管理工作流，他的基石是AB，所以他拥有之前AB所有的功能。并且他已经帮助开发者实现了资源管理系统。
## 打包对比

### AB打包方式

AB使用BuildPipeline.BuildAssetBundles（string outputPath , BuildAssetBundleOptions options, BuildTarget target)接口对所有标记为ab资产的资源进行打包。
outputPath：打包后的资源输出路径
options：使用什么方式对资产进行打包。
BuildAssetBundleOptions.None：此捆绑包选项使用LZMA格式压缩。这样打包后的资源占用磁盘空间最小，但是使用时必须解压缩整个捆绑包一次，加载速度会更慢。
BuildAssetBundleOptions.UncompressedAssetBundle:此包选项以完全未压缩数据的方式构建包。解压缩的缺点是文件下载量较大。但是，一旦下载，加载时间将最快。
BuildAssetBundleOptions.ChunkBasedCompression：此捆绑包选项使用一种称为LZ4的压缩方法，与LZMA相比，其压缩文件大小更大，但与LZMA不同，不需要使用整个捆绑包进行解压缩后才能使用。LZ4使用基于块的算法，该算法允许以片段或“块”的形式加载AB。
使用ChunkBasedCompression具有与未压缩的包相当的加载时间，并具有减小磁盘大小的额外好处。

target：此参数决定了当前构建的捆绑包用于哪一个目标平台。

AB文件

这是一个缺少.manifest扩展名的文件，以及您在运行时要加载的资源，以加载资产。

AB文件是一个存档，在内部包含多个文件。此存档的结构可能会略有变化，具体取决于它是普通AB还是场景AB。这是普通AB的结构：


场景AB与普通的AB不同，因为它已针对场景及其内容的流加载进行了优化。

清单文件
对于每个生成的捆绑包，包括附加的清单捆绑包，都会生成一个关联的清单文件。清单文件可以使用任何文本编辑器打开，并且包含诸如捆绑包的循环冗余校验（CRC）数据和依赖项数据之类的信息。对于普通的AB，其清单文件将如下所示：

```
ManifestFileVersion: 0
CRC: 2422268106
Hashes:
  AssetFileHash:
    serializedVersion: 2
    Hash: 8b6db55a2344f068cf8a9be0a662ba15
  TypeTreeHash:
    serializedVersion: 2
    Hash: 37ad974993dbaa77485dd2a0c38f347a
HashAppended: 0
ClassTypes:
- Class: 91
  Script: {instanceID: 0}
Assets:
  Asset_0: Assets/Mecanim/StateMachine.controller
Dependencies: {}
```
### Addressable打包方式

Addressable跟AB的打包方式有所不同，因为它可以选择种中播放模式，如下图所示

Fast Mode: 快速模式：直接加载文件而不打包，快速但Profiler获取的信息较少；在此模式下，我们实际上时使用 AssetDatabase.LoadAssetAtPath 直接加载文件。
Virtual Mode:虚拟模式：在不打包的情况下模拟靠近AB的操作；与FastMode不同，您可以查看哪个AB包含资产。此模式最终还加载了AssetDatabase.LoadAssetAtPath 。
Packed Mode:打包模式：实际上是从AB打包和加载；在这种模式下，实际构建并加载AB。
如果在编辑器模式下使用Addressable推荐使用Virtual Mode来模拟打包加载，在真正要上线期间再使用Packed Play Mode 来进行打包加载。

## 加载资源对比

### AB加载方式

我们用AB来加载资源的过程中，主要包含两个过程，第一步是先要加载本地或者网络上的AB资产包，再通过操作这个AB来加载它里面所包含的资源。

加载AB有以下三种方式：

```
public static AssetBundle LoadFromFile(string path, uint crc, ulong offset);
```

path	磁盘上文件的路径。
crc	未压缩内容的可选CRC-32校验。如果它不为零，则在加载内容之前将其与校验和进行比较，如果不匹配则给出错误。
offset	可选的字节偏移量。该值指定从何处开始读取AB。
这是本地磁盘加载到内存中所以是加载ab包最快的方式，它可以加载任意压缩形式的ab包。

```
public static AssetBundle LoadFromMemory(byte[] binary, uint crc);
```

binary	具有AB数据的字节数组。
crc	未压缩内容的可选CRC-32校验和。如果它不为零，则在加载内容之前将其与校验和进行比较，如果不匹配则给出错误。
​ 使用此方法从字节数组创建AB。当下载带有加密的数据并且需要从未加密的字节创建AB时，这很有用。

public static Networking.UnityWebRequest GetAssetBundle(string uri, uint version, uint crc);

uri	要下载的资产捆绑包的URI
version	版本号，它将与要下载的资产捆绑包的缓存版本进行比较。如果不一样则则将重新下载资产捆绑。
crc	版本哈希。如果此哈希与该资产捆绑的缓存版本的哈希不匹配，则将重新下载资产捆绑。
创建一个优化的UnityWebRequest，用于通过HTTP GET下载Unity资产捆绑包。

从AB中加载资源的方式

public <T> LoadAsset<T> (string name);

name:资源名称

T:资源类型

该接口可以同步的加载资源

public AssetBundleRequest LoadAssetAsync(string name);

name:资源名称

该接口可以异步的加载资源

### Addressable加载方式

我们首先把资源标记为可寻址，可寻址资源系统会给我们一个它的地址，然后我们可以根据这个地址去异步加载资源。可寻址资源系统的加载资源方式都是异步的，所以我们需要监听它加载完成再使用。

通过代码编写

public static AsyncOperationHandle <T> LoadAssetAsync<T>(object key);

key：资源的地址
AsyncOperationHandle ：监听异步操作的句柄
public static void InstantiateAsync(object key);

key:资源的地址

该接口可以直接通过地址来实例化物体

通过AssetReference 访问可寻址资产而不需要知道它的地址

我们在继承自monobehaviour的脚步中添加一个成员：

public AssetReference ar;
然后在该脚本组件的视图中就可以选择可寻址资产了。


这样我们就无需知道该可寻址资源的地址也可以直接加载它。

//先加载后面自己控制实例化
ar.LoadAssetAsync<T>().Completed += LoadComplete;
//直接实例化
ar.InstantiateAsync();
以上就是可寻址资源系统最简单的加载一个资源的方法。

我们也可以通过传入好几个地址或者标签来加载一系列资源。在可寻址资源系统中我们可以定义一系列标签，然后再给可寻址资源贴上相应标签，然后我们通过标签来加载资源时会直接加载所有贴上该标签的资源。**

public static AsyncOperationHandle<IList<T>> LoadAssetsAsync<T>(object key, Action<T> callback);

返回值 AsyncOperationHandle：异步句柄
key:标签
callback：每个资源被加载完成后会调用的回调
public static AsyncOperationHandle<IList<T>> LoadAssetsAsync<T>(IList<object> keys, Action<T> callback, MergeMode mode);

返回值 AsyncOperationHandle : 异步句柄
keys:所有要加载的资源的地址
callback:每个资源被加载完成后会调用的回调
mode:最后返回的所有资源
## 卸载资源方式对比

### AB卸载方式

该图可以清晰的描述AB加载后的内存分布和不同的卸载方式对应卸载哪一些区域。

我们通过本地或者网络加载后都会有一份内存镜像，我们需要通过它再来加载资源，如果我们调用了AssetBundle.Unload(false)那么内存镜像被释放，我们也不能再通过这个ab包来加载资源，但是之前加载的资源不会被释放，我们可以通过Re'sources.UnloadAsset(obj)来释放加载的资源，如果想要释放所有从ab包加载的资源那么就需要调用AssetBundle.Unload(true)，如果这个时候还有其他资源引用了ab包加载的资源，那么就会造成引用丢失造成显示不正常。

### Addressable卸载方式

由于Addressable是在AB的基础上使用的系统，所以它的内存分布跟AB差不多，但是由于我们不能拿到资源对应的ab对象，只能通过Addressable给出的接口释放资源。Addressable内部已经给我们做了引用计数管理，所有当我们释放对应资源时，只有当引用计数为0才会真正的卸载对应ab对象。

不能实例化的资源卸载方式

1.public static void Release<T>(T obj);

obj:资源
​ 直接释放T类型的资源，并且减少引用计数

2.public static void Release(AsyncOperationHandle handle);

handle：句柄
​ 释放由该句柄加载出来的资源，并减少引用计数

能实例化的资源卸载方式

1.public static bool ReleaseInstance(GameObject instance);

instance:实例化的资源

销毁游戏物体，并且减少引用计数。如果用该方法卸载不是由寻址系统实例化的物体，那么释放不会产生影响。

2.public static bool ReleaseInstance(AsyncOperationHandle handle);

handle：句柄

释放由该句柄加载出来的资源，并减少引用计数

这边需要注意一点，实例化资源时可以传一个参数trackHandle默认为true。如果他为true那么销毁实例化的资源用上面2个接口都可以，如果为false那么只能用第二个接口。

## 热更资源方式对比

### AB热更资源方式

热更资源在Addressable推出之前每个项目都有自己的做法，但是大致上都可以抽象为以下流程。

先给每一个ab记录一个版本号，这个版本号可以是md5、hash值或者版本数字。但是最好是一个版本数字，防止项目迁移之后资源计算出的md5或者hash发生改变从而导致重新下载一遍相同资源。然后将所有包名对应的版本号记录在一个资源配置文件里。
第一次进入游戏没有这个资源配置文件，先下载资源配置文件，再把配置文件里的所有ab下载到本地。
之后登陆游戏时对比本地资源配置文件里的资源版本号和远程资源配置文件里的资源版本号，如果版本号小了，那么记录这个ab。最后把所有需要更新的ab下载并且更新本地文件。
资源配置文件除了可以记录版本，还可以附加一些信息比如ab大小，所以我们下载时可以弹窗提示玩家需要更新多少大的文件。

### Addressable热更资源方式

Addressable也支持资源热更，但是还是有很多不足的地方。我用了它的热更功能然后看了它热更功能的实现后发现它更加适合边下边玩，而不是在游戏刚进去时更新完所有资源。原因有以下几点：

因为我们把下载ab的实现交给了addressable，然后它的实现是当你在加载资源时找到这个资源的ab包，然后通过UnityWebRequestAssetBundle判断该ab包是不是已经下载如果下载那么直接从缓存目录加载，不然就下载到缓存目录再加载。所以我们要先加载资源才会去下载ab包。
目前没有提供接口来知道那些ab包需要热更。
虽然它本身不支持游戏初始化时下载所有资源，但是因为addressable是开源的，我们可以直接修改源码来达到我们自己想要的效果。比如我们想在游戏刚开始得到所有需要更新ab的大小，我们可以通过以下代码来实现

AddressablesImpl.cs

``` C#
//添加2个新方法
//初始化完之后调用GetRemoteBundleSizeAsync方法
AsyncOperationHandle<long> GetRemoteBundleSizeWithChain(IList<string> bundles)
{
    return ResourceManager.CreateChainOperation(InitializationOperation, op => GetRemoteBundleSizeAsync(key));
}
//通过ab包名得到ab包大小
public AsyncOperationHandle<long> GetRemoteBundleSizeAsync(IList<string> bundles)
{
    //如果还没初始化完那么等待初始化完
    if(!InitializationOperation.IsDone)
        return GetRemoteBundleSizeWithChain(key);
    IList<IResourceLocation> locations = new IList<IResourceLocation>();
    for(var i = 0; i < bundles.Count, i++)
    {
        IList<IResourceLocation> tmpLocations;
        var key = bundles[i];
        //寻找传入的包名对应的ab包，如果没找到那么警告
       if(!GetResourceLocations(key, typeof(object), out locations))
        return ResourceManager.CreateCompletedOperation<Long>(0, new InvalidKeyException(key).Message);
        locations.Add(tmpLocations[0]);
    }
    //总的包大小
    long size = 0;
    for(var i = 0; i < locations.Count; i++)
    {
        if(locations[i].Data != null)
        {
            var sizeData = locations[i].Data as ILocationSizeData;
            if(sizeData != null)
            {
                //计算包大小
                size += sizeData.ComputeSize(locations[i]);
            }
        }
    }
    //返回总的包大小
    return ResourceManager.CreateCompletedOperation<Long>(size, string.Empty)
}
//在对应的Addressables外观类里也添加GetRemoteBundleSizeAsync方法
 
//使用方法
//在addressable初始化完成后遍历所有地址，如果地址的结尾是.bundle，那么他对应了一个ab包，然后把它缓存到列表，再使用添加的接口来获得所有需要更新包的大小。
Addressables.InitializeAsync().Completed += opHandle =>
{
    var map = opHandle.Result as ResourceLocationMap;
    List<string> bundles = new List<string>();
    foreach (object mapKey in map.keys)
    {
        string key = mapKey as string;
        if(key != null && key.EndsWith(".bundle"))
        {
            bundles.Add(key);
        }
    }
    Addressables.GetRemoteBundleSizeAsync(key).Completed += asyncOpHandle => print(asyncOpHandle.Result);
};
```