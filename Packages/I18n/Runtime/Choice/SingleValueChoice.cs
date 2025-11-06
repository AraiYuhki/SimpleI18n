namespace Xeon.Localization
{
    /// <summary>単一値一致判定を行う選択肢</summary>
    public class SingleValueChoice : IChoice
    {
        /// <summary>表示テキスト</summary>
        public string Text { get; }

        private readonly int _value;
        /// <summary>単一値選択肢を構築する</summary>
        public SingleValueChoice(string text, int value)
        {
            Text = text;
            _value = value;
        }

        /// <summary>値が一致するか判定する</summary>
        public bool IsMatch(int value)
            => _value == value;
    }
}
