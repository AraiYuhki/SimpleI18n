namespace Xeon.Localization
{
    /// <summary>上限以下判定を行う選択肢</summary>
    public class BelowValueChoice : IChoice
    {
        /// <summary>表示テキスト</summary>
        public string Text { get; }
        private readonly int _max;
        /// <summary>上限以下選択肢を構築する</summary>
        public BelowValueChoice(string text, int max)
        {
            Text = text;
            _max = max;
        }

        /// <summary>値が上限以下か判定する</summary>
        public bool IsMatch(int value)
            => value <= _max;
    }
}
