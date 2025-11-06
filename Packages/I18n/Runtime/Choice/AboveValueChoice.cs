namespace Xeon.Localization
{
    /// <summary>下限以上判定を行う選択肢</summary>
    public class AboveValueChoice : IChoice
    {
        /// <summary>表示テキスト</summary>
        public string Text { get; }
        private readonly int _min;
        /// <summary>下限以上選択肢を構築する</summary>
        public AboveValueChoice(string text, int min)
        {
            Text = text;
            _min = min;
        }

        /// <summary>値が下限以上か判定する</summary>
        public bool IsMatch(int value)
            => _min <= value;
    }
}
