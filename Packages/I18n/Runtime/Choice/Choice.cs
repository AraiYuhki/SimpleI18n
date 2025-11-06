namespace Xeon.Localization
{
    /// <summary>選択肢評価インターフェース</summary>
    public interface IChoice
    {
        /// <summary>表示テキスト</summary>
        string Text { get; }
        /// <summary>値が条件に一致するか</summary>
        bool IsMatch(int value);
    }
}
