using System;

namespace Xeon.Localization
{
    /// <summary>
    /// 言語ごとのアセットをキーで取得するためのインターフェース
    /// </summary>
    /// <typeparam name="TData">取得対象アセット型</typeparam>
    /// <typeparam name="TEnum">言語識別用列挙型</typeparam>
    public interface IAssetDatabase<TData, TEnum>
        where TData : UnityEngine.Object
        where TEnum : Enum
    {
        /// <summary>指定言語のアセットをキーから取得する 存在しない場合は null 推奨</summary>
        /// <param name="language">取得したい言語</param>
        /// <param name="key">アセットキー null/空は未定義</param>
        /// <returns>アセットまたは null</returns>
        TData Translate(TEnum language, string key);

        /// <summary>デフォルト言語または内部ルールでアセットを取得する</summary>
        /// <param name="key">アセットキー</param>
        /// <returns>アセットまたは null</returns>
        TData Translate(string key);
        
        /// <summary>指定言語でアセット取得を試みる</summary>
        /// <param name="language">取得言語</param>
        /// <param name="key">アセットキー</param>
        /// <param name="result">取得結果 成功時アセット 失敗時 null/既定値</param>
        /// <returns>成功なら true 見つからなければ false</returns>
        bool TryTranslate(TEnum language, string key, out TData result);

        /// <summary>デフォルト言語でアセット取得を試みる</summary>
        /// <param name="key">アセットキー</param>
        /// <param name="result">取得結果 成功時アセット</param>
        /// <returns>成功なら true 失敗なら false</returns>
        bool TryTranslate(string key, out TData result);
    }
}
