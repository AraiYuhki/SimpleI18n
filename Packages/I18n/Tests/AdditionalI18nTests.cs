using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Xeon.Localization.Test
{
    /// <summary>追加テスト: フォールバック/パラメータ/選択肢/アセット DB エッジケース</summary>
    public class AdditionalI18nTests
    {
        private I18n<TestData, SystemLanguage> _i18n;
        private TestDatabase _db;

        [SetUp]
        public void SetUp()
        {
            _db = new TestDatabase();
            _i18n = new I18n<TestData, SystemLanguage>(_db, SystemLanguage.Japanese);
        }

        /// <summary>存在しない言語要求でデフォルト言語へフォールバックする</summary>
        [Test]
        public void FallbackToDefaultLanguageTest()
        {
            LogAssert.Expect(LogType.Warning, "translate_onlyのデータにEnglishが存在しなかったのでJapaneseにフォールバックしました");
            var result = _i18n.Translate(SystemLanguage.English, "translate_only");
            Assert.That(result, Is.EqualTo("translated text"));
        }

        /// <summary>キー未存在時はキーそのものを返す (null と空文字)</summary>
        [Test]
        public void MissingKeyReturnsOriginalKey()
        {
            string nullKey = null;
            Assert.That(_i18n.Translate(nullKey), Is.Null);
            Assert.That(_i18n.Translate(""), Is.EqualTo(""));
        }

        /// <summary>複数回出現するプレースホルダが全て置換される</summary>
        [Test]
        public void MultiplePlaceholderReplacementTest()
        {
            // 事前に DB へ動的追加 (日本語のみで十分)
            var dataField = typeof(TestDatabase).GetField("_data", BindingFlags.NonPublic | BindingFlags.Static);
            var dict = (Dictionary<string, TestData>)dataField.GetValue(null);
            dict["repeat_placeholder"] = new TestData((SystemLanguage.Japanese, "value :x and again :x end"));
            var result = _i18n.Translate("repeat_placeholder", (":x".Trim(':'), 42));
            Assert.That(result, Is.EqualTo("value 42 and again 42 end"));
        }

        /// <summary>選択肢がどれも一致しない場合 2番目の選択肢へフォールバックする仕様を検証</summary>
        [Test]
        public void ChoiceFallbackSecondSegmentTest()
        {
            var dataField = typeof(TestDatabase).GetField("_data", BindingFlags.NonPublic | BindingFlags.Static);
            var dict = (Dictionary<string, TestData>)dataField.GetValue(null);
            dict["choice_fallback"] = new TestData((SystemLanguage.Japanese, "{0} first|{1} second|[5,*] large"));
            // 値 3 はどれにも一致しない -> 2番目(second)期待
            var result = _i18n.TransChoice("choice_fallback", 3);
            Assert.That(result, Is.EqualTo("second"));
        }

        /// <summary>Above / Below パターン選択肢が境界を正しく評価する</summary>
        [Test]
        public void AboveBelowChoiceParsingTest()
        {
            var dataField = typeof(TestDatabase).GetField("_data", BindingFlags.NonPublic | BindingFlags.Static);
            var dict = (Dictionary<string, TestData>)dataField.GetValue(null);
            dict["above_below"] = new TestData((SystemLanguage.Japanese, "[*,2] small|{3} three|[4,*] big"));
            Assert.That(_i18n.TransChoice("above_below", 1), Is.EqualTo("small"));
            Assert.That(_i18n.TransChoice("above_below", 2), Is.EqualTo("small"));
            Assert.That(_i18n.TransChoice("above_below", 3), Is.EqualTo("three"));
            Assert.That(_i18n.TransChoice("above_below", 4), Is.EqualTo("big"));
            Assert.That(_i18n.TransChoice("above_below", 10), Is.EqualTo("big"));
        }

        /// <summary>不正な範囲 (min>=max) は例外を投げる</summary>
        [Test]
        public void InvalidRangeThrows()
        {
            Assert.Throws<InvalidDataException>(() => new RangeValueChoice("text", 5, 5));
            Assert.Throws<InvalidDataException>(() => new RangeValueChoice("text", 6, 5));
        }

        #region Asset Database Tests
        private class DummySpriteDatabase : IAssetDatabase<Sprite, SystemLanguage>
        {
            private readonly Dictionary<string, Sprite> _jp = new();
            public DummySpriteDatabase()
            {
                var tex = new Texture2D(4,4);
                tex.SetPixel(0,0, Color.red); tex.Apply();
                _jp["icon"] = Sprite.Create(tex, new Rect(0,0,4,4), new Vector2(0.5f,0.5f));
            }
            public Sprite Translate(SystemLanguage language, string key)
                => TryTranslate(language, key, out var s) ? s : null;
            public Sprite Translate(string key)
                => TryTranslate(SystemLanguage.Japanese, key, out var s) ? s : null;
            public bool TryTranslate(SystemLanguage language, string key, out Sprite result)
            {
                if (language == SystemLanguage.Japanese && _jp.TryGetValue(key, out result)) return true;
                result = null; return false;
            }
            public bool TryTranslate(string key, out Sprite result)
                => TryTranslate(SystemLanguage.Japanese, key, out result);
        }

        /// <summary>未登録時に Sprite 翻訳 API が例外を投げる</summary>
        [Test]
        public void SpriteDatabaseNotRegisteredThrows()
        {
            Assert.Throws<InvalidOperationException>(() => _i18n.TranslateSprite("icon"));
        }

        /// <summary>Sprite データベース登録後に取得成功する</summary>
        [Test]
        public void SpriteDatabaseTranslateSuccess()
        {
            _i18n.RegisterSpriteDatabase(new DummySpriteDatabase());
            var sprite = _i18n.TranslateSprite("icon");
            Assert.That(sprite, Is.Not.Null);
        }

        /// <summary>存在しないキーは null を返す</summary>
        [Test]
        public void SpriteDatabaseMissingKeyReturnsNull()
        {
            _i18n.RegisterSpriteDatabase(new DummySpriteDatabase());
            var sprite = _i18n.TranslateSprite("missing");
            Assert.That(sprite, Is.Null);
        }
        #endregion
    }
}

