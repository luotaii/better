# Furigana Plugin

为日语歌词的汉字标注假名（振假名）的 BetterLyrics 插件。

## 功能特点

- 🎯 **精准标注**：使用 MeCab 分词器准确识别日语汉字并标注正确读音
- 📝 **自定义词典**：支持用户自定义特殊读音
- 🎨 **保持原样**：非汉字部分（假名、英文、标点）保持原样输出
- ⏱️ **时间轴支持**：兼容 LRC 歌词时间轴格式

## 使用方法

1. 安装插件后，在 BetterLyrics 的 transliteration 设置中选择 "Furigana"
2. 目标语言代码设置为 `ja-furigana`
3. 插件会自动为日语歌词中的汉字添加假名标注

## 输出格式

汉字会以 `汉字 (假名)` 的格式显示，例如：
- `東京` → `東京 (とうきょう)`
- `日本` → `日本 (にほん)`
- `私は学生です` → `私 (わたし) は 学生 (がくせい) です`

## 自定义词典

编辑 `customizeDict.txt` 文件可以添加自定义读音，格式为：
```
漢字 仮名
```

例如：
```
東京 とうきょう
大阪 おおさか
```

## 开发说明

本插件基于 BetterLyrics 插件系统开发，参考了 Romaji 插件的实现。

### 项目结构

```
BetterLyrics.Plugins.Transliteration.Furigana/
├── Config.cs                 # 插件配置
├── Plugin.cs                 # 插件主入口
├── Helpers/
│   ├── FuriganaHelper.cs    # 核心转换逻辑
│   └── KanaHelper.cs        # 假名转换工具
├── Models/
│   ├── ConvertedUnit.cs     # 转换单元模型
│   └── ReplaceString.cs     # 替换字符串模型
└── customizeDict.txt        # 自定义词典
```

## 依赖

- MeCab.DotNet: 日语分词库
- BetterLyrics.Core: BetterLyrics 核心库

## 许可证

遵循 BetterLyrics 项目的 GPL v3.0 许可证
