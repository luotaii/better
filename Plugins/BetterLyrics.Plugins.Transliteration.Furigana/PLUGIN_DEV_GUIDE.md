# BetterLyrics 插件开发指南

## 概述

BetterLyrics 是一个基于 WinUI3 的歌词可视化与音乐播放应用，支持通过插件系统扩展功能。本指南将帮助你了解如何开发一个为日语汉字标注假名的插件。

## 插件架构

### 核心接口

BetterLyrics 插件系统使用以下核心接口：

1. **IPlugin** - 所有插件的基础接口
2. **ILyricsTransliterator** - 歌词转写/注音接口

### 基类

- **PluginBase<TConfig>** - 插件基类，提供配置、上下文等基础功能

## 创建 Furigana 插件

### 1. 项目结构

```
BetterLyrics.Plugins.Transliteration.Furigana/
├── BetterLyrics.Plugins.Transliteration.Furigana.csproj  # 项目文件
├── Config.cs                                              # 配置类
├── Plugin.cs                                              # 插件入口
├── README.md                                              # 说明文档
├── customizeDict.txt                                      # 自定义词典
├── Helpers/
│   ├── FuriganaHelper.cs                                 # 核心转换逻辑
│   └── KanaHelper.cs                                     # 假名工具类
├── Models/
│   ├── ConvertedUnit.cs                                  # 转换单元模型
│   └── ReplaceString.cs                                  # 替换选项模型
└── Langs/
    ├── zh-Hans.json                                      # 中文语言包
    ├── en.json                                           # 英文语言包
    └── ja.json                                           # 日文语言包
```

### 2. 项目文件 (.csproj)

关键配置项：

```xml
<PropertyGroup>
    <TargetFramework>net10.0-windows10.0.26100.0</TargetFramework>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <MeCabUseDefaultDictionary>False</MeCabUseDefaultDictionary>
    <Description>Add furigana annotations to Japanese kanji in lyrics</Description>
</PropertyGroup>
```

依赖项：
- MeCab.DotNet: 日语分词库
- BetterLyrics.Core: 核心库引用

### 3. 插件主类 (Plugin.cs)

实现 `ILyricsTransliterator` 接口：

```csharp
public class Plugin : PluginBase<Config>, ILyricsTransliterator
{
    public override string Title { get; set; } = "Furigana";

    protected override async Task OnInitializeAsync()
    {
        FuriganaHelper.Init(Context.PluginDirectory);
    }

    public Task<string?> GetTransliterationAsync(
        string text, 
        string targetLangCode, 
        CancellationToken token)
    {
        // 当 targetLangCode == "ja-furigana" 时处理
        if (targetLangCode == "ja-furigana")
        {
            // 处理逻辑
        }
        return Task.FromResult(result);
    }
}
```

### 4. 配置类 (Config.cs)

```csharp
public class Config : PluginConfigBase
{
    public bool OnlyKanji { get; set; } = true;
    public bool UseHiragana { get; set; } = true;
}
```

### 5. 核心转换逻辑 (FuriganaHelper.cs)

#### 初始化 MeCab 分词器

```csharp
public static void Init(string baseDirectory)
{
    var dicPath = Path.Combine(baseDirectory, "unidic");
    var parameter = new MeCabParam
    {
        DicDir = dicPath,
        LatticeLevel = MeCabLatticeLevel.Zero
    };
    _tagger = MeCabTagger.Create(parameter);
    
    // 加载自定义词典
    LoadCustomDictionary(baseDirectory);
}
```

#### 转换流程

1. **解析歌词行**：提取时间轴和文本
2. **语言检测**：跳过纯中文或纯英文行
3. **分词处理**：使用 MeCab 分词
4. **获取读音**：从 MeCab 节点提取假名
5. **生成结果**：组合汉字和假名

```csharp
public static IEnumerable<ConvertedLine> ToFurigana(string text)
{
    // 分割行
    var lines = text.Split(Environment.NewLine);
    
    foreach (var line in lines)
    {
        // 跳过非日语文本
        if (IsChinese(line, 0.8f)) continue;
        
        // 分词并标注
        foreach (var node in _tagger.ParseToNodes(line))
        {
            var unit = MeCabNodeToUnit(lineIndex, node);
            convertedLine.Units.Add(unit);
        }
        
        yield return convertedLine;
    }
}
```

### 6. MeCab 节点处理

```csharp
public static ConvertedUnit MeCabNodeToUnit(ushort lineIndex, MeCabNode item)
{
    if (TryCustomConvert(item.Surface, out var customResult))
    {
        // 使用自定义词典
        return new ConvertedUnit(lineIndex, item.Surface, customResult, true);
    }
    else if (IsJapanese(item.Surface))
    {
        // 纯假名
        return new ConvertedUnit(lineIndex, item.Surface, item.Surface, false);
    }
    else if (IsEnglish(item.Surface))
    {
        // 英文
        return new ConvertedUnit(lineIndex, item.Surface, item.Surface, false);
    }
    else
    {
        // 汉字：获取读音
        var kana = GetKana(item);
        return new ConvertedUnit(lineIndex, item.Surface, kana, true);
    }
}
```

### 7. 自定义词典

格式：`汉字 假名`

```
東京 とうきょう
大阪 おおさか
一気呵成 いっかかせい
```

## 输出格式

插件返回的文本格式为：
- 汉字：`汉字 (假名)`
- 假名/英文：保持原样

示例：
```
入力：私は東京で日本語を勉強します
出力：私 (わたし) は 東京 (とうきょう) で 日本語 (にほんご) を 勉強 (べんきょう) します
```

## 语言代码

- `ja-furigana`: 日语振假名模式

## 构建和打包

### 构建命令

```bash
dotnet build BetterLyrics.Plugins.Transliteration.Furigana.csproj
```

### 自动生成

项目文件中的 Target 会自动：
1. 生成修剪配置文件
2. 打包为 .blp 文件

### 安装包位置

构建后会在 `Plugins/_Dist/` 目录生成 `.blp` 文件。

## 测试

### 单元测试示例

```csharp
[TestMethod]
public void Test_Furigana_Conversion()
{
    FuriganaHelper.Init(testDirectory);
    
    var input = "東京";
    var result = FuriganaHelper.ToFurigana(input).FirstOrDefault();
    
    Assert.AreEqual("東京", result.Units[0].Japanese);
    Assert.AreEqual("とうきょう", result.Units[0].Hiragana);
    Assert.IsTrue(result.Units[0].IsKanji);
}
```

## 常见问题

### Q: 如何处理多音字？
A: MeCab 会根据上下文自动选择正确的读音。如需强制指定，使用自定义词典。

### Q: 为什么某些汉字没有标注？
A: 可能是：
1. 被识别为中文（检查 IsChinese 方法）
2. MeCab 词典中不存在
3. 标点符号或特殊字符

### Q: 如何优化性能？
A: 
1. 复用 MeCabTagger 实例（已实现）
2. 避免重复初始化
3. 使用异步处理长文本

## 参考资源

- [MeCab.DotNet 文档](https://github.com/kekyo/MeCab.DotNet)
- [UniDic 词典](https://ccd.ninjal.ac.jp/unidic/)
- [BetterLyrics Romaji 插件](../BetterLyrics.Plugins.Transliteration.Romaji/)

## 贡献指南

1. Fork 项目
2. 创建功能分支
3. 提交更改
4. 推送到分支
5. 创建 Pull Request

## 许可证

遵循 BetterLyrics 项目的 GPL v3.0 许可证
