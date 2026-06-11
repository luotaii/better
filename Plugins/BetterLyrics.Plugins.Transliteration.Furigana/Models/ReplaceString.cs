using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BetterLyrics.Plugins.Transliteration.Furigana.Models
{
    /// <summary>
    /// 表示可替换的假名选项
    /// </summary>
    public class ReplaceString : INotifyPropertyChanged
    {
        private ushort _id;
        private string _text;
        private bool _isSelected;

        public ReplaceString(ushort id, string text, bool isSelected = false)
        {
            Id = id;
            Text = text;
            IsSelected = isSelected;
        }

        public ushort Id
        {
            get => _id;
            set
            {
                if (value == _id) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                if (value == _text) return;
                _text = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value == _isSelected) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
