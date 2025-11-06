using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xeon.Localization
{
    /// <summary>
    /// 汎用翻訳サービス 文字列キーからデータを取得し指定言語で文字列やアセットを返す
    /// </summary>
    /// <typeparam name="TData">翻訳データ型 (言語別テキストを保持)</typeparam>
    /// <typeparam name="TEnum">言語列挙またはキー型</typeparam>
    public class I18n<TData, TEnum>
        where TData : ITranslateDataSet<TEnum>
        where TEnum : Enum
    {
        /// <summary>翻訳用基礎データベース</summary>
        private IDatabase<TData, TEnum> _database;
        /// <summary>Sprite 用アセットデータベース 未登録なら利用不可</summary>
        private IAssetDatabase<Sprite, TEnum> _spriteDatabase;
        /// <summary>Font 用アセットデータベース 未登録なら利用不可</summary>
        private IAssetDatabase<Font, TEnum> _fontDatabase;
        /// <summary>Prefab 用アセットデータベース 未登録なら利用不可</summary>
        private IAssetDatabase<GameObject, TEnum> _prefabDatabase;

        /// <summary>翻訳データベースがセット済みか</summary>
        public bool IsInitialized => _database != null;
        /// <summary>
        /// 翻訳データベースを差し替える
        /// </summary>
        /// <param name="database">新しいデータベース</param>
        public void SetDatabase(IDatabase<TData, TEnum> database)
            => _database = database;

        /// <summary>
        /// 言語フォールバックに使うデフォルト言語
        /// </summary>
        public TEnum DefaultLanguage { get; set; } = default;

        /// <summary>
        /// 継承用保護コンストラクタ
        /// </summary>
        protected I18n() { }

        /// <summary>
        /// 翻訳サービスを初期化する
        /// </summary>
        /// <param name="database">翻訳データベース</param>
        /// <param name="defaultLanguage">フォールバック言語</param>
        public I18n(IDatabase<TData, TEnum> database, TEnum defaultLanguage)
        {
            _database = database;
            DefaultLanguage = defaultLanguage;
        }

        /// <summary>Sprite データベースを登録</summary>
        public void RegisterSpriteDatabase(IAssetDatabase<Sprite, TEnum> database)
            => _spriteDatabase = database;
        /// <summary>Sprite データベースを解除</summary>
        public void UnregisterSpriteDatabase() => _spriteDatabase = null;

        /// <summary>Font データベースを登録</summary>
        public void RegisterFontDatabase(IAssetDatabase<Font, TEnum> database)
            => _fontDatabase = database;
        /// <summary>Font データベースを解除</summary>
        public void UnregisterFontDatabase() => _fontDatabase = null;
        
        /// <summary>Prefab データベースを登録</summary>
        public void RegisterPrefabDatabase(IAssetDatabase<GameObject, TEnum> database)
            => _prefabDatabase = database;
        /// <summary>Prefab データベースを解除</summary>
        public void UnregisterPrefabDatabase() => _prefabDatabase = null;

        /// <summary>
        /// デフォルト言語で文字列を翻訳する 失敗時はキーを返す
        /// </summary>
        /// <param name="key">翻訳キー</param>
        /// <param name="param">埋め込みパラメータ (":name" -> 値)</param>
        /// <returns>翻訳文字列またはキー</returns>
        public string Translate(string key, params (string, object)[] param)
        {
            if (!_database.TryFindByKey(key, out var data)) return key;
            if (!data.Translate(DefaultLanguage, out var result)) return key;
            return ReplaceParam(result, param);
        }

        /// <summary>
        /// 指定言語で文字列を翻訳する 未対応ならフォールバック
        /// </summary>
        /// <param name="lang">要求言語</param>
        /// <param name="key">翻訳キー</param>
        /// <param name="param">埋め込みパラメータ</param>
        /// <returns>翻訳文字列またはキー</returns>
        public string Translate(TEnum lang, string key, params (string, object)[] param)
        {
            if (!_database.TryFindByKey(key, out var data)) return key;
            if (data.Translate(lang, out var result)) return ReplaceParam(result, param);
            if (data.Translate(DefaultLanguage, out result))
            {
                Debug.LogWarning($"{key}のデータに{lang}が存在しなかったので{DefaultLanguage}にフォールバックしました");
                return ReplaceParam(result, param);
            }
            Debug.LogError($"{key}のデータに{lang}と{DefaultLanguage}の情報が存在しませんでした");
            return key;
        }

        /// <summary>
        /// 選択肢付きテキストを翻訳する フォーマット '...|...|...'
        /// </summary>
        /// <param name="key">翻訳キー</param>
        /// <param name="select">選択基準値</param>
        /// <param name="param">埋め込みパラメータ</param>
        /// <returns>選択された翻訳文字列またはキー</returns>
        public string TransChoice(string key, int select, params (string, object)[] param)
        {
            if (!_database.TryFindByKey(key, out var data)) return key;
            if (!data.Translate(DefaultLanguage, out var result)) return key;
            return Parse(result, select, param);
        }

        /// <summary>
        /// 指定言語で選択肢付き翻訳を行う 未対応ならフォールバック
        /// </summary>
        /// <param name="lang">要求言語</param>
        /// <param name="key">翻訳キー</param>
        /// <param name="select">選択基準値</param>
        /// <param name="param">埋め込みパラメータ</param>
        /// <returns>選択された翻訳文字列またはキー</returns>
        public string TransChoice(TEnum lang, string key, int select, params (string, object)[] param)
        {
            if (!_database.TryFindByKey(key, out var data)) return key;
            if (data.Translate(lang, out var result)) return Parse(result, select, param);
            if (data.Translate(DefaultLanguage, out result))
            {
                Debug.LogWarning($"{key}のデータに{lang}が存在しなかったので{DefaultLanguage}にフォールバックしました");
                return Parse(result, select, param);
            }
            Debug.LogError($"{key}のデータに{lang}と{DefaultLanguage}のデータが存在しませんでした");
            return key;
        }

        /// <summary>Sprite をデフォルト言語で取得</summary>
        /// <param name="key">アセットキー</param>
        /// <returns>Sprite または null</returns>
        /// <exception cref="InvalidOperationException">Sprite DB 未登録</exception>
        public Sprite TranslateSprite(string key)
        {
            ThrowIfSpriteDatabaseIsNotRegistered();
            return _spriteDatabase.Translate(key);
        }

        /// <summary>指定言語の Sprite を取得</summary>
        public Sprite TranslateSprite(TEnum language, string key)
        {
            ThrowIfSpriteDatabaseIsNotRegistered();
            return _spriteDatabase.Translate(language, key);
        }

        /// <summary>Sprite の Try 取得 (デフォルト)</summary>
        public bool TryTranslateSprite(string key, out Sprite result)
        {
            result = null;
            ThrowIfSpriteDatabaseIsNotRegistered();
            return _spriteDatabase.TryTranslate(key, out result);
        }

        /// <summary>Sprite の Try 取得 (指定言語)</summary>
        public bool TryTranslateSprite(TEnum language, string key, out Sprite result)
        {
            result = null;
            ThrowIfSpriteDatabaseIsNotRegistered();
            return _spriteDatabase.TryTranslate(language, key, out result);
        }

        /// <summary>Font をデフォルト言語で取得</summary>
        public Font TranslateFont(string key)
        {
            ThrowIfFontDatabaseIsNotRegistered();
            return _fontDatabase.Translate(key);
        }

        /// <summary>指定言語の Font を取得</summary>
        public Font TranslateFont(TEnum language, string key)
        {
            ThrowIfFontDatabaseIsNotRegistered();
            return _fontDatabase.Translate(language, key);
        }

        /// <summary>Font の Try 取得 (デフォルト)</summary>
        public bool TryTranslateFont(string key, out Font result)
        {
            result = null;
            ThrowIfFontDatabaseIsNotRegistered();
            return _fontDatabase.TryTranslate(key, out result);
        }

        /// <summary>Font の Try 取得 (指定言語)</summary>
        public bool TryTranslateFont(TEnum language, string key, out Font result)
        {
            result = null;
            ThrowIfFontDatabaseIsNotRegistered();
            return _fontDatabase.TryTranslate(language, key, out result);
        }

        /// <summary>Prefab をデフォルト言語で取得</summary>
        public GameObject TranslatePrefab(string key)
        {
            ThrowIfPrefabDatabaseIsNotRegistered();
            return _prefabDatabase.Translate(key);
        }

        /// <summary>指定言語の Prefab を取得</summary>
        public GameObject TranslatePrefab(TEnum language, string key)
        {
            ThrowIfPrefabDatabaseIsNotRegistered();
            return _prefabDatabase.Translate(language, key);
        }

        /// <summary>Prefab の Try 取得 (デフォルト)</summary>
        public bool TryTranslatePrefab(string key, out GameObject result)
        {
            result = null;
            ThrowIfPrefabDatabaseIsNotRegistered();
            return _prefabDatabase.TryTranslate(key, out result);
        }

        /// <summary>Prefab の Try 取得 (指定言語)</summary>
        public bool TryTranslatePrefab(TEnum language, string key, out GameObject result)
        {
            result = null;
            ThrowIfPrefabDatabaseIsNotRegistered();
            return _prefabDatabase.TryTranslate(language, key, out result);
        }

        /// <summary>Sprite DB 未登録なら例外</summary>
        private void ThrowIfSpriteDatabaseIsNotRegistered()
        {
            if (_spriteDatabase == null)
                throw new InvalidOperationException("Sprite Database is not registered.");
        }

        /// <summary>Font DB 未登録なら例外</summary>
        private void ThrowIfFontDatabaseIsNotRegistered()
        {
            if (_fontDatabase == null)
                throw new InvalidOperationException("Font Database is not registered.");
        }

        /// <summary>Prefab DB 未登録なら例外</summary>
        private void ThrowIfPrefabDatabaseIsNotRegistered()
        {
            if (_prefabDatabase == null)
                throw new InvalidOperationException("Prefab Database is not registered.");
        }
        
        /// <summary>
        /// 選択肢形式テキストを解析し最適なものを選ぶ
        /// </summary>
        /// <param name="original">'|' 区切り原文</param>
        /// <param name="select">選択基準値</param>
        /// <param name="param">埋め込みパラメータ</param>
        /// <returns>選択されたテキスト</returns>
        private string Parse(string original, int select, params (string, object)[] param)
        {
            if (!original.Contains("|"))
            {
                return ReplaceParam(original, param);
            }

            var splitted = original.Split("|");
            var choices = new List<IChoice>();
            foreach ((var text, var index) in splitted.Select((text, index) => (text, index)))
            {
                choices.Add(ChoiceFactory.CreateChoice(text, index));
            }
            // どの選択肢にも当てはまらない場合は2つ目の選択肢を選ぶ
            var choice = choices.FirstOrDefault(choice => choice.IsMatch(select)) ?? choices[1];
            return ReplaceParam(choice.Text, param);
        }

        /// <summary>
        /// テキスト内プレースホルダをパラメータ値へ置換する
        /// </summary>
        /// <param name="text">元テキスト</param>
        /// <param name="param">(キー, 値) ペア</param>
        /// <returns>置換後テキスト</returns>
        private string ReplaceParam(string text, params (string, object)[] param)
        {
            if (param == null) return text;

            var result = text;
            foreach ((var key, var value) in param)
                result = result.Replace($":{key}", value.ToString());

            return result;
        }
    }
}
