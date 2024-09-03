using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Xeon.Localization.Test
{
    public class TestData : ITranslateDataSet<SystemLanguage>
    {
        private Dictionary<SystemLanguage, string> _texts = new Dictionary<SystemLanguage, string>();
        public TestData(string text) => _texts.Add(SystemLanguage.Japanese, text);

        public TestData(params (SystemLanguage lang, string text)[] texts)
        {
            _texts = texts.ToDictionary(pair => pair.lang, pair => pair.text);
        }

        public string Text => _texts[SystemLanguage.Japanese];
        public bool Translate(SystemLanguage lang, out string result)
        {
            return _texts.TryGetValue(lang, out result);
        }
    }

    public class TestDatabase : IDatabase<TestData, SystemLanguage>
    {
        private static readonly Dictionary<string, TestData> _data = new()
        {
            { "translate_only" , new TestData("translated text") },
            { "choice_test", new TestData("{0} zero|{1} one|[2,5] between two and five|[6,*] greater than six") },
            { "inline_argument_test", new TestData("argument replace test value = :value") },
            { "inline_arguments_test", new TestData("arguments replace test value1 = :value1 and value2 = :value2") },
            { "full_function_test", new TestData("{0} first choice result value = :value|[2,5] second choice, value is between 2 <= :value <= 5|[6,*] third choice the :value greater than six") },
            { "range_test", new TestData("[*,-1]value is minus|{0}value is zero|[1,*]value is plus") },
            { "parse_failed_data", new TestData("{1,9}failed data|[10]failed data 2") },
            { "parse_failed_between_data", new TestData("[0] first choice|[5,2] second choice") },
            { "explicit_translate_test", new TestData((SystemLanguage.Japanese, "日本語"), (SystemLanguage.English, "英語"), (SystemLanguage.Chinese, "中国語")) },
            { "rank_test", new TestData((SystemLanguage.Japanese, "{1}優勝|{2}準優勝|[3,5]:rank位(敢闘賞)|[6,*]:rank位"), (SystemLanguage.English, "{1}1st|{2}2nd|{3}3rd|[4,*]:rankth")) },
            { "german_only", new TestData((SystemLanguage.German, "ドイツ語")) },
        };
        public TestData FindByKey(string key)
        {
            return _data[key];
        }

        public bool TryFindByKey(string key, out TestData translated)
        {
            return _data.TryGetValue(key, out translated);
        }
    }

    public class I18nTest
    {
        private I18n<TestData, SystemLanguage> i18n;
        [OneTimeSetUp]
        public void SetupDatabase()
        {
            var database = new TestDatabase();
            i18n = new I18n<TestData, SystemLanguage>(database, SystemLanguage.Japanese);
        }

        public class TestCaseData
        {
            public string Key { get; }
            public (string, object)[] Params { get; }
            public int? Choice { get; } = null;
            public SystemLanguage Lang { get; } = SystemLanguage.Japanese;
            public string Expect { get; }

            private string description = string.Empty;

            public TestCaseData(string key, string except, string description, int choice, params (string, object)[] param)
            {
                Key = key;
                Choice = choice;
                Expect = except;
                Params = param;
                this.description = description;
            }

            public TestCaseData(string key, string except, string description, params (string, object)[] param)
            {
                Key = key;
                Params = param;
                Expect = except;
                this.description = description;
            }

            public TestCaseData(string key, string except, string description, SystemLanguage lang, params (string, object)[] param)
            {
                Key = key;
                Expect = except;
                Lang = lang;
                Params = param;
                this.description = description;
            }

            public TestCaseData(string key, string except, string description, int choice, SystemLanguage lang, params (string, object)[] param)
            {
                Key = key;
                Expect = except;
                Lang = lang;
                Params = param;
                Choice = choice;
                this.description = description;

            }

            public override string ToString()
            {
                var paramText = "null";
                if (Params.Length > 0)
                {
                    paramText = string.Join(",", Params.Select(tuple => $"{tuple.Item1} = {tuple.Item2}"));
                }

                var messageList = new List<string>() { description };
                if (Choice.HasValue)
                    messageList.Add($"choice = {Choice.Value}");
                messageList.Add($"param = ({paramText})");
                messageList.Add($"lang = {Lang}");
                return string.Join(",", messageList.ToArray());
            }

            public static IEnumerable<TestCaseData> GetTranslateTestData()
            {
                yield return new TestCaseData("translate_only", "translated text", "翻訳のみのテスト");
                yield return new TestCaseData("translate_only", "translated text", "パラメータ付きの翻訳テスト", ("value", "test"));
                yield return new TestCaseData("inline_argument_test", "argument replace test value = test", "単一パラメータ置換テスト", ("value", "test"));
                yield return new TestCaseData("inline_argument_test", "argument replace test value = :value", "単一パラメータ置換テスト", ("value1", "test"));
                yield return new TestCaseData("inline_argument_test", "argument replace test value = argument2", "単一パラメータ置換テスト", ("argument1", "argument1"), ("value", "argument2"));
                yield return new TestCaseData("inline_arguments_test", "arguments replace test value1 = 1 and value2 = 2", "複数パラメータ置換テスト", ("value1", 1), ("value2", 2));
                yield return new TestCaseData("inline_arguments_test", "arguments replace test value1 = 1 and value2 = :value2", "複数パラメータ置換テスト", ("value1", 1), ("test2", 2));
                yield return new TestCaseData("inline_arguments_test", "arguments replace test value1 = :value1 and value2 = :value2", "複数パラメータ置換テスト");
                yield return new TestCaseData("choice_test", "{0} zero|{1} one|[2,5] between two and five|[6,*] greater than six", "選択肢ありのテスト");
                yield return new TestCaseData("full_function_test", "{0} first choice result value = :value|[2,5] second choice, value is between 2 <= :value <= 5|[6,*] third choice the :value greater than six", "全機能テスト");
                yield return new TestCaseData("test", "test", "キーが存在しない場合のテスト");
            }

            public static IEnumerable<TestCaseData> GetTransChoiceTestData()
            {
                yield return new TestCaseData("translate_only", "translated text", "選択肢なし", 0);
                yield return new TestCaseData("choice_test", "zero", "選択肢3つ(1つ目の選択肢)", 0);
                yield return new TestCaseData("choice_test", "one", "選択肢3つ(2つ目)", 1);
                yield return new TestCaseData("choice_test", "between two and five", "選択肢4つ(3つ目)", 2);
                yield return new TestCaseData("choice_test", "between two and five", "選択肢4つ(3つ目)", 4);
                yield return new TestCaseData("choice_test", "between two and five", "選択肢4つ(3つ目)", 5);
                yield return new TestCaseData("choice_test", "greater than six", "選択肢4つ(4つ目)", 6);
                yield return new TestCaseData("choice_test", "greater than six", "選択肢4つ(4つ目)", 100);
                yield return new TestCaseData("choice_test", "one", "選択肢3つ(該当なし)", -1);
                yield return new TestCaseData("full_function_test", "first choice result value = 10", "全機能テスト(選択肢１つ目)", 0, ("value", 10));
                yield return new TestCaseData("full_function_test", "first choice result value = :value", "全機能テスト(選択肢１つ目)", 0);
                yield return new TestCaseData("full_function_test", "first choice result value = test", "全機能テスト(選択肢１つ目)", 0, ("0", "zero"), ("value", "test"));
                yield return new TestCaseData("full_function_test", "second choice, value is between 2 <= 2 <= 5", "全機能テスト(選択肢2つ目)", 2, ("value", 2));
                yield return new TestCaseData("full_function_test", "second choice, value is between 2 <= 3 <= 5", "全機能テスト(選択肢2つ目)", 3, ("value", 3));
                yield return new TestCaseData("full_function_test", "second choice, value is between 2 <= 5 <= 5", "全機能テスト(選択肢2つ目)", 5, ("value", 5));
                yield return new TestCaseData("full_function_test", "third choice the 6 greater than six", "全機能テスト(選択肢3つ目)", 6, ("value", 6));
                yield return new TestCaseData("full_function_test", "third choice the 999 greater than six", "全機能テスト(選択肢3つ目)", 999, ("value", 999));
                yield return new TestCaseData("full_function_test", "second choice, value is between 2 <= -1 <= 5", "全機能テスト(該当なし)", -1, ("value", -1));
                yield return new TestCaseData("range_test", "value is minus", "範囲テスト", -10);
                yield return new TestCaseData("range_test", "value is minus", "範囲テスト", -1);
                yield return new TestCaseData("range_test", "value is zero", "範囲テスト", 0);
                yield return new TestCaseData("range_test", "value is plus", "範囲テスト", 1);
                yield return new TestCaseData("range_test", "value is plus", "範囲テスト", 10);
                yield return new TestCaseData("ignore key", "ignore key", "キーが存在しない", 0);
            }

            public static IEnumerable<TestCaseData> GetExplicitTransTestData()
            {
                yield return new TestCaseData("explicit_translate_test", "日本語", "明示的に言語を選択するテスト", SystemLanguage.Japanese);
                yield return new TestCaseData("explicit_translate_test", "英語", "明示的に言語を選択するテスト", SystemLanguage.English);
                yield return new TestCaseData("explicit_translate_test", "中国語", "明示的に言語を選択するテスト", SystemLanguage.Chinese);
                yield return new TestCaseData("explicit_translate_test", "日本語", "明示的に言語を選択するテスト", SystemLanguage.German);
                yield return new TestCaseData("german_only", "ドイツ語", "明示的に言語を選択するテスト", SystemLanguage.German);
            }

            public static IEnumerable<TestCaseData> GetExplicitTransChoiceTestData()
            {
                yield return new TestCaseData("rank_test", "優勝", "変換込みの明示的に言語を選択するテスト", 1, SystemLanguage.Japanese, ("rank", 1));
                yield return new TestCaseData("rank_test", "準優勝", "変換込みの明示的に言語を選択するテスト", 2, SystemLanguage.Japanese, ("rank", 2));
                for(var rank = 3; rank <= 5; rank++)
                {
                    yield return new TestCaseData("rank_test", $"{rank}位(敢闘賞)", "変換込みの明示的に言語を選択するテスト", rank, SystemLanguage.Japanese, ("rank", rank));
                }
                yield return new TestCaseData("rank_test", $"6位", "変換込みの明示的に言語を選択するテスト", 6, SystemLanguage.Japanese, ("rank", 6));

                yield return new TestCaseData("rank_test", "1st", "変換込みの明示的に言語を選択するテスト", 1, SystemLanguage.English, ("rank", 1));
                yield return new TestCaseData("rank_test", "2nd", "変換込みの明示的に言語を選択するテスト", 2, SystemLanguage.English, ("rank", 2));
                yield return new TestCaseData("rank_test", "3rd", "変換込みの明示的に言語を選択するテスト", 3, SystemLanguage.English, ("rank", 3));
                for (var rank = 4; rank < 6; rank++)
                {
                    yield return new TestCaseData("rank_test", $"{rank}th", "変換込みの明示的に言語を選択するテスト", rank, SystemLanguage.English, ("rank", rank));
                }
            }
        }

        [Test]
        [TestCaseSource(typeof(TestCaseData), nameof(TestCaseData.GetTranslateTestData))]
        public void TranslateTest(TestCaseData testData)
        {
            Assert.That(testData.Expect == i18n.Translate(testData.Key, testData.Params));
        }

        [Test]
        [TestCaseSource(typeof(TestCaseData), nameof(TestCaseData.GetTransChoiceTestData))]
        public void TransChoiceTest(TestCaseData testData)
        {
            var actual = i18n.TransChoice(testData.Key, testData.Choice.Value, testData.Params);
            Debug.Log(actual);
            Assert.That(testData.Expect == actual);
        }

        [Test]
        public void ParseFailedBetweenDataTest()
        {
            const string Key = "parse_failed_between_data";
            Assert.That(() => i18n.TransChoice(Key, 0), Throws.TypeOf<InvalidDataException>().With.Message.EqualTo("最小値が最大値を超えています"));
        }

        [Test]
        [TestCaseSource(typeof(TestCaseData), nameof(TestCaseData.GetExplicitTransTestData))]
        public void ExplicitTransTest(TestCaseData testData)
        {
            var actual = i18n.Translate(testData.Lang, testData.Key, testData.Params);
            Assert.That(testData.Expect == actual);
        }

        [Test]
        [TestCaseSource(typeof(TestCaseData), nameof(TestCaseData.GetExplicitTransChoiceTestData))]
        public void ExplicitTransChoiceTest(TestCaseData testData)
        {
            var actual = i18n.TransChoice(testData.Lang, testData.Key, testData.Choice.Value, testData.Params);
            Assert.That(testData.Expect == actual);
        }

        [Test]
        public void ExplicitFallbackFailedTest()
        {
            LogAssert.Expect(LogType.Error, "german_onlyのデータにEnglishとJapaneseの情報が存在しませんでした");
            Assert.That("german_only" == i18n.Translate(SystemLanguage.English, "german_only"));

            LogAssert.Expect(LogType.Error, "german_onlyのデータにChineseとJapaneseのデータが存在しませんでした");
            Assert.That("german_only" == i18n.TransChoice(SystemLanguage.Chinese, "german_only", 0));
        }
    }
}
