using System;

namespace Xeon.Localization
{
    /// <summary>
    /// 翻訳データをキーで検索するためのインターフェース
    /// </summary>
    /// <typeparam name="TData">翻訳対象データ型</typeparam>
    /// <typeparam name="TEnum">言語識別用列挙型</typeparam>
    public interface IDatabase<TData, TEnum>
        where TData : ITranslateDataSet<TEnum>
        where TEnum : Enum
    {
        /// <summary>キーに一致する翻訳データを取得する 見つからない場合は例外</summary>
        /// <param name="key">検索するキー null/空不可</param>
        /// <returns>一致したデータ</returns>
        /// <exception cref="ArgumentException">key が null または空文字</exception>
        /// <exception cref="KeyNotFoundException">該当キーなし</exception>
        TData FindByKey(string key);
        /// <summary>キーに一致する翻訳データを取得する 成功時 true 失敗時 false</summary>
        /// <param name="key">検索するキー</param>
        /// <param name="translated">取得されたデータ 失敗時は既定値</param>
        /// <returns>成功なら true 未存在なら false</returns>
        bool TryFindByKey(string key, out TData translated);
    }
}
