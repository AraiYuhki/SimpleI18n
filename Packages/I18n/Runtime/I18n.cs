using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Xeon.Localization
{
    public class I18n<TData, TEnum>
        where TData : ITranslateDataSet<TEnum>
        where TEnum : Enum
    {
        private IDatabase<TData, TEnum> _database;

        public bool IsInitialized => _database != null;
        public void SetDatabase(IDatabase<TData, TEnum> database)
            => _database = database;

        public TEnum DefaultLanguage { get; set; } = default;

        public I18n(IDatabase<TData, TEnum> database, TEnum defaultLanguage)
        {
            _database = database;
            DefaultLanguage = defaultLanguage;
        }

        public string Translate(string key, params (string, object)[] param)
        {
            if (!_database.TryFindByKey(key, out var data)) return key;
            return ReplaceParam(data.Text, param);
        }

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
        /// 選択肢付きの翻訳
        /// </summary>
        /// <param name="key">翻訳情報のキー</param>
        /// <param name="select">どの選択肢を使うか？</param>
        /// <param name="param">文言中に埋め込むパラメータ</param>
        /// <returns></returns>
        public string TransChoice(string key, int select, params (string, object)[] param)
        {
            if (!_database.TryFindByKey(key, out var data)) return key;
            return Parse(data.Text, select, param);
        }

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
        /// 文言にパラメータを埋め込む
        /// </summary>
        /// <param name="text"></param>
        /// <param name="param"></param>
        /// <returns></returns>
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
