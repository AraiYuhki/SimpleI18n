namespace Xeon.Localization
{
    /// <summary>
    /// キー付きエントリを表す最小インターフェース
    /// </summary>
    public interface IKeyEntry
    {
        /// <summary>エントリを一意に識別するキー</summary>
        string Key { get; }
    }
}
