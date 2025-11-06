using System.IO;

namespace Xeon.Localization
{
    /// <summary>指定範囲内判定を行う選択肢</summary>
    public class RangeValueChoice : IChoice
    {
        /// <summary>表示テキスト</summary>
        public string Text { get; }
        private readonly int _min;
        private readonly int _max;
        /// <summary>範囲選択肢を構築する</summary>
        public RangeValueChoice(string text, int min, int max)
        {
            Text = text;
            _min = min;
            _max = max;
            Validate();
        }

        /// <summary>範囲の妥当性を検証する</summary>
        private void Validate()
        {
            if (_min >= _max)
            {
                throw new InvalidDataException("最小値が最大値を超えています");
            }
        }

        /// <summary>値が範囲内か判定する</summary>
        public bool IsMatch(int value)
            => _min <= value && value <= _max;
    }
}
