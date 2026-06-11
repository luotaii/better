using BetterLyrics.Plugins.Transliteration.Furigana.Models;
using MeCab;
using MeCab.Extension.UniDic;
using System.Text;
using System.Text.RegularExpressions;

namespace BetterLyrics.Plugins.Transliteration.Furigana.Helpers
{
    /// <summary>
    /// 为日语汉字标注假名（振假名）的核心帮助类
    /// </summary>
    public static class FuriganaHelper
    {
        /// <summary>
        /// 分词器
        /// </summary>
        private static MeCabTagger? _tagger;

        /// <summary>
        /// 自定义词典<原文，假名>
        /// </summary>
        private static Dictionary<string, string>? _customizeDict;

        /// <summary>
        /// 初始化分词器和词典
        /// </summary>
        public static void Init(string? baseDirectory = null)
        {
            string rootPath = !string.IsNullOrEmpty(baseDirectory)
                     ? baseDirectory
                     : AppDomain.CurrentDomain.BaseDirectory;

            // 词典路径
            var dicPath = Path.Combine(rootPath, "unidic");
            var parameter = new MeCabParam
            {
                DicDir = dicPath,
                LatticeLevel = MeCabLatticeLevel.Zero
            };
            _tagger = MeCabTagger.Create(parameter);

            // 加载自定义词典
            var customizeDictPath = Path.Combine(rootPath, "customizeDict.txt");
            if (File.Exists(customizeDictPath))
            {
                var str = File.ReadAllText(customizeDictPath);
                var list = str.Split(Environment.NewLine.ToArray());
                _customizeDict = new Dictionary<string, string>();
                foreach (var item in list)
                {
                    if (string.IsNullOrWhiteSpace(item)) continue;
                    var array = item.Split(' ');
                    if (array.Length < 2) continue;
                    if (!_customizeDict.ContainsKey(array[0]))
                        _customizeDict.Add(array[0], array[1]);
                }
            }
            else
            {
                _customizeDict = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// 将文本转换为带假名标注的结果列表
        /// </summary>
        public static IEnumerable<ConvertedLine> ToFurigana(string text)
        {
            var timeSpans = new List<TimeSpan?>();
            var lineTextList = text.Split(Environment.NewLine.ToArray())
                .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

            // 解析歌词时间轴
            for (var i = 0; i < lineTextList.Count; i++)
            {
                if (LrcLineRegex.IsMatch(lineTextList[i]))
                {
                    var lyric = ParseLrcLine(lineTextList[i]).FirstOrDefault();
                    timeSpans.Add(lyric.Time);
                    lineTextList[i] = lyric.Text;
                }
                else
                {
                    timeSpans.Add(null);
                }
            }

            ushort lineIndex = 0;
            for (var index = 0; index < lineTextList.Count; index++)
            {
                var line = lineTextList[index];

                // 跳过纯中文行
                if (IsChinese(line, 0.8f)) continue;

                var convertedLine = new ConvertedLine
                {
                    Index = lineIndex,
                    Time = index < timeSpans.Count ? timeSpans[index] : null,
                    Japanese = line.Replace("\0", "")
                };

                // 处理每个句子
                foreach (var sentence in convertedLine.Japanese.LineToUnits())
                {
                    if (IsEnglish(sentence))
                    {
                        // 英文直接添加
                        convertedLine.Units.Add(new ConvertedUnit(lineIndex, sentence, sentence, false));
                    }
                    else if (IsJapanese(sentence))
                    {
                        // 纯假名直接添加
                        convertedLine.Units.Add(new ConvertedUnit(lineIndex, sentence, sentence, false));
                    }
                    else
                    {
                        // 包含汉字的句子，使用 MeCab 分词
                        foreach (var unit in SentenceToFurigana(lineIndex, sentence))
                            convertedLine.Units.Add(unit);
                    }
                }

                lineIndex++;
                yield return convertedLine;
            }
        }

        /// <summary>
        /// 将句子转换为带假名标注的单元列表
        /// </summary>
        public static IEnumerable<ConvertedUnit> SentenceToFurigana(ushort lineIndex, string str)
        {
            foreach (var item in _tagger.ParseToNodes(str))
            {
                var unit = MeCabNodeToUnit(lineIndex, item);
                if (unit != null)
                    yield return unit;
            }
        }

        /// <summary>
        /// 将 MeCab 节点转换为转换单元
        /// </summary>
        public static ConvertedUnit MeCabNodeToUnit(ushort lineIndex, MeCabNode item)
        {
            ConvertedUnit unit = null;
            if (item.CharType > 0)
            {
                if (TryCustomConvert(item.Surface, out var customResult))
                {
                    // 用户自定义词典
                    unit = new ConvertedUnit(lineIndex,
                        item.Surface,
                        KanaHelper.ToHiragana(customResult),
                        true);
                }
                else if (IsJapanese(item.Surface))
                {
                    // 纯假名
                    unit = new ConvertedUnit(lineIndex,
                        item.Surface,
                        KanaHelper.ToHiragana(item.Surface),
                        false);
                }
                else if (IsEnglish(item.Surface))
                {
                    // 英文
                    unit = new ConvertedUnit(lineIndex,
                        item.Surface,
                        item.Surface,
                        false);
                }
                else
                {
                    // 汉字或其他字符
                    var kana = GetKana(item);
                    unit = new ConvertedUnit(lineIndex,
                        item.Surface,
                        KanaHelper.ToHiragana(kana),
                        !IsJapanese(item.Surface));
                }
            }
            else if (item.Stat != MeCabNodeStat.Bos && item.Stat != MeCabNodeStat.Eos)
            {
                unit = new ConvertedUnit(lineIndex,
                    item.Surface,
                    item.Surface,
                    false);
            }

            return unit;
        }

        /// <summary>
        /// 获取假名读音
        /// </summary>
        private static string GetKana(MeCabNode node)
        {
            return node.GetPos1() == "助詞" ? node.GetPron() : node.GetKana();
        }

        /// <summary>
        /// 尝试使用自定义词典转换
        /// </summary>
        private static bool TryCustomConvert(string str, out string result)
        {
            if (_customizeDict != null && _customizeDict.ContainsKey(str))
            {
                result = _customizeDict[str];
                return true;
            }

            result = "";
            return false;
        }

        /// <summary>
        /// 判断字符串是否简体中文
        /// </summary>
        public static bool IsChinese(string str, float rate)
        {
            if (str.Length < 2)
                return false;

            var wordArray = str.ToCharArray();
            var total = wordArray.Length;
            var chCount = 0f;
            var enCount = 0f;

            foreach (var word in wordArray)
            {
                if (word != 'ー' && IsJapanese(word.ToString()))
                    return false;

                var gbBytes = Encoding.Unicode.GetBytes(word.ToString());

                if (gbBytes.Length == 2)
                {
                    if (gbBytes[1] >= 0x4E && gbBytes[1] <= 0x9F)
                        chCount++;
                    else
                        total--;
                }
                else if (gbBytes.Length == 1)
                {
                    var byteAscii = int.Parse(gbBytes[0].ToString());
                    if ((byteAscii >= 65 && byteAscii <= 90) || (byteAscii >= 97 && byteAscii <= 122))
                        enCount++;
                    else
                        total--;
                }
            }

            if (chCount == 0) return false;

            return (chCount + enCount) / total >= rate;
        }

        /// <summary>
        /// 判断字符串是否全为单字节（英文）
        /// </summary>
        public static bool IsEnglish(string str)
        {
            return new Regex("^[\\x20-\\x7E]+$", RegexOptions.Compiled).IsMatch(str);
        }

        /// <summary>
        /// 判断字符串是否全为假名
        /// </summary>
        private static bool IsJapanese(string str)
        {
            return Regex.IsMatch(str, @"^[\u3040-\u30ff]+$", RegexOptions.Compiled);
        }

        /// <summary>
        /// 歌词行正则表达式
        /// </summary>
        private static readonly Regex LrcLineRegex = new Regex(@"^\[(\d{2}):(\d{2})\.(\d{2,3})\](.*)", RegexOptions.Compiled);

        /// <summary>
        /// 解析歌词行
        /// </summary>
        private static IEnumerable<(TimeSpan? Time, string Text)> ParseLrcLine(string line)
        {
            var match = LrcLineRegex.Match(line);
            if (match.Success)
            {
                var minutes = int.Parse(match.Groups[1].Value);
                var seconds = int.Parse(match.Groups[2].Value);
                var milliseconds = int.Parse(match.Groups[3].Value.PadRight(3, '0'));
                var time = new TimeSpan(0, minutes, seconds, milliseconds);
                var text = match.Groups[4].Value;
                yield return (time, text);
            }
            else
            {
                yield return (null, line);
            }
        }
    }

    /// <summary>
    /// 表示转换后的一行
    /// </summary>
    public class ConvertedLine
    {
        public ushort Index { get; set; }
        public TimeSpan? Time { get; set; }
        public string Japanese { get; set; } = string.Empty;
        public string Chinese { get; set; } = string.Empty;
        public List<ConvertedUnit> Units { get; set; } = new List<ConvertedUnit>();
    }

    /// <summary>
    /// 字符串扩展方法
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 将句子按标点符号分割
        /// </summary>
        public static IEnumerable<string> LineToUnits(this string line)
        {
            var units = new List<string>();
            var currentUnit = new StringBuilder();

            foreach (var c in line)
            {
                if (char.IsPunctuation(c) || char.IsSymbol(c))
                {
                    if (currentUnit.Length > 0)
                    {
                        units.Add(currentUnit.ToString());
                        currentUnit.Clear();
                    }
                    units.Add(c.ToString());
                }
                else
                {
                    currentUnit.Append(c);
                }
            }

            if (currentUnit.Length > 0)
            {
                units.Add(currentUnit.ToString());
            }

            return units;
        }
    }
}
