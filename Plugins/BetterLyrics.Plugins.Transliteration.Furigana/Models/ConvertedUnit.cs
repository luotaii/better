using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BetterLyrics.Plugins.Transliteration.Furigana.Models
{
    /// <summary>
    /// 表示转换后的单个单元（汉字或假名）
    /// </summary>
    public class ConvertedUnit : INotifyPropertyChanged
    {
        private string? _hiragana;
        private bool _isKanji;
        private string? _japanese;
        private ObservableCollection<ReplaceString>? _replaceHiragana;
        private ushort _selectId;
        private ushort _lineIndex;

        public ConvertedUnit(ushort lineIndex, string japanese, string hiragana, bool isKanji)
        {
            LineIndex = lineIndex;
            Japanese = japanese;
            Hiragana = hiragana;
            IsKanji = isKanji;
            SelectId = 1;
            ReplaceHiragana = new ObservableCollection<ReplaceString> { new ReplaceString(1, hiragana, true) };
        }

        public ushort LineIndex
        {
            get => _lineIndex;
            set
            {
                if (value == _lineIndex) return;
                _lineIndex = value;
                OnPropertyChanged();
            }
        }

        public string? Japanese
        {
            get => _japanese;
            set
            {
                if (value == _japanese) return;
                _japanese = value;
                OnPropertyChanged();
            }
        }

        public string? Hiragana
        {
            get => _hiragana;
            set
            {
                if (value == _hiragana) return;
                _hiragana = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ReplaceString>? ReplaceHiragana
        {
            get => _replaceHiragana;
            set
            {
                if (Equals(value, _replaceHiragana)) return;
                _replaceHiragana = value;
                OnPropertyChanged();
            }
        }

        public bool IsKanji
        {
            get => _isKanji;
            set
            {
                if (value == _isKanji) return;
                _isKanji = value;
                OnPropertyChanged();
            }
        }

        public ushort SelectId
        {
            get => _selectId;
            set
            {
                if (value == _selectId) return;
                _selectId = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
