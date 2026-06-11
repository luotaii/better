using BetterLyrics.Core.Abstractions;
using BetterLyrics.Core.Interfaces.Features;
using BetterLyrics.Plugins.Transliteration.Furigana.Helpers;

namespace BetterLyrics.Plugins.Transliteration.Furigana
{
    /// <summary>
    /// 为日语歌词的汉字标注假名（振假名）插件
    /// </summary>
    public class Plugin : PluginBase<Config>, ILyricsTransliterator
    {
        public override string Title { get; set; } = "Furigana";

        protected override async Task OnInitializeAsync()
        {
            FuriganaHelper.Init(Context.PluginDirectory);
        }

        public Task<string?> GetTransliterationAsync(string text, string targetLangCode, CancellationToken token)
        {
            string? result = null;
            
            // 当目标语言代码为 "ja-furigana" 时，为日语汉字标注假名
            if (targetLangCode == "ja-furigana")
            {
                var lines = text.Split("\n");
                result = string.Join("\n", lines.Select(line => 
                {
                    if (string.IsNullOrWhiteSpace(line))
                        return line;
                    
                    // 使用 FuriganaHelper 处理每一行
                    var convertedLines = FuriganaHelper.ToFurigana(line);
                    return string.Join(" ", convertedLines.FirstOrDefault()?.Units.Select(unit => 
                    {
                        // 如果是汉字，返回 [汉字] (假名) 格式
                        if (unit.IsKanji && !string.IsNullOrEmpty(unit.Hiragana))
                        {
                            return $"{unit.Japanese}({unit.Hiragana})";
                        }
                        // 如果不是汉字，直接返回原文
                        return unit.Japanese;
                    }) ?? line);
                }));
            }
            
            return Task.FromResult(result);
        }
    }
}
