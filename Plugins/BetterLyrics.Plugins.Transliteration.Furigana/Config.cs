using BetterLyrics.Core.Abstractions;

namespace BetterLyrics.Plugins.Transliteration.Furigana
{
    public class Config : PluginConfigBase
    {
        /// <summary>
        /// 是否仅为汉字标注假名（不处理已经是假名的部分）
        /// </summary>
        public bool OnlyKanji { get; set; } = true;

        /// <summary>
        /// 假名显示格式：平假名 (true) 或 片假名 (false)
        /// </summary>
        public bool UseHiragana { get; set; } = true;
    }
}
