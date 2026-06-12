using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.Util
{
    public class UniversalHighlighter
    {
        public const string CyrillicAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
        public const string LatinAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public const string AtbashCyrillicTop = "А Б В Г Д Е Ё Ж З И Й К Л М Н О П Р С Т У Ф Х Ц Ч Ш Щ Ъ Ы Ь Э Ю Я";
        public const string AtbashCyrillicBottom = "Я Ю Э Ь Ы Ъ Щ Ш Ч Ц Х Ф У Т С Р П О Н М Л К Й И З Ж Ё Е Д Г В Б А";

        public const string Rot13Top = "N O P Q R S T U V W X Y Z A B C D E F G H I J K L M";
        public const string Rot13Bottom = "A B C D E F G H I J K L M N O P Q R S T U V W X Y Z";

        private readonly string _validChars;
        private readonly string _topAlphabet;
        private readonly string _bottomAlphabet;
        private const string HighlightColor = "#FFFF00";

        private Label _topLabel;
        private Label _bottomLabel;

        public UniversalHighlighter(Label topLabel, Label bottomLabel, string validChars, string topAlphabet, string bottomAlphabet)
        {
            _topLabel = topLabel;
            _bottomLabel = bottomLabel;
            _validChars = validChars;
            _topAlphabet = topAlphabet;
            _bottomAlphabet = bottomAlphabet;

            if (_topLabel != null) _topLabel.style.letterSpacing = 0;
            if (_bottomLabel != null) _bottomLabel.style.letterSpacing = 0;
        }

        public void HighlightCharacter(char letter)
        {
            int index = _validChars.IndexOf(char.ToUpper(letter));
            if (index >= 0)
            {
                HighlightAtIndex(index, index);
            }
        }

        public void HighlightCharacter(char letter, bool reverseBottom)
        {
            int index = _validChars.IndexOf(char.ToUpper(letter));
            if (index >= 0)
            {
                HighlightAtIndex(index, reverseBottom ? (_validChars.Length - 1 - index) : index);
            }
        }

        public void HighlightAtIndex(int topIndex, int bottomIndex)
        {
            if (_topLabel != null) _topLabel.text = HighlightCharAt(_topAlphabet, topIndex);
            if (_bottomLabel != null) _bottomLabel.text = HighlightCharAt(_bottomAlphabet, bottomIndex);
        }

        public void HighlightIndices(IEnumerable<int> indices)
        {
            if (indices == null)
            {
                Reset();
                return;
            }
            var set = new HashSet<int>(indices.Where(i => i >= 0));
            if (_topLabel != null) _topLabel.text = HighlightCharsAt(_topAlphabet, set);
            if (_bottomLabel != null) _bottomLabel.text = HighlightCharsAt(_bottomAlphabet, set);
        }

        public void Reset()
        {
            if (_topLabel != null) 
            {
                _topLabel.text = _topAlphabet;
                _topLabel.style.opacity = 1f;
            }
            if (_bottomLabel != null)
            {
                _bottomLabel.text = _bottomAlphabet;
                _bottomLabel.style.opacity = 0.7f;
            }
        }

        private string HighlightCharAt(string text, int targetIndex)
        {
            return HighlightCharsAt(text, new HashSet<int> { targetIndex });
        }

        private string HighlightCharsAt(string text, HashSet<int> targets)
        {
            int charCount = 0;
            var result = new StringBuilder();
            
            foreach (char c in text)
            {
                if (_validChars.Contains(char.ToUpper(c).ToString()))
                {
                    if (targets.Contains(charCount))
                    {
                        result.Append($"<color={HighlightColor}>{c}</color>");
                    }
                    else
                    {
                        result.Append(c);
                    }
                    charCount++;
                }
                else
                {
                    result.Append(c);
                }
            }
            
            return result.ToString();
        }
    }
}
