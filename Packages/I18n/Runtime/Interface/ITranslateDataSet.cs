using System;

namespace Xeon.Localization
{
    /// <summary>
    /// 翻訳可能なデータセットの最小インターフェース
    /// </summary>
    /// <typeparam name="T">言語識別用の列挙型</typeparam>
    public interface ITranslateDataSet<T> where T : Enum
    {
        /// <summary>指定した言語の文字列を取得する</summary>
        /// <param name="language">取得対象の言語</param>
        /// <param name="result">取得できた文字列 存在しない場合は <c>null</c></param>
        /// <returns>文字列が存在すれば true 存在しなければ false</returns>
        bool Translate(T language, out string result);
    }
}
