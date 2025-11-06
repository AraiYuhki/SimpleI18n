using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Xeon.Localization
{
    /// <summary>範囲や単一値などのパターン文字列から IChoice 実装を生成するファクトリ</summary>
    public static class ChoiceFactory
    {
        private delegate IChoice CreateMethod(string text);

        private static readonly Regex SingleRegex = new Regex(@"\{([-\d]+)\}");
        private static readonly Regex RangeRegex = new Regex(@"\[([-\d]+),([-\d]+)\]");
        private static readonly Regex AboveRegex = new Regex(@"\[([-\d]+),(\*)\]");
        private static readonly Regex BelowRegex = new Regex(@"\[(\*),([-\d]+)\]");

        private static readonly List<(Regex regex, CreateMethod method)> CreateList = new()
        {
            ( SingleRegex, CreateSingle ),
            ( RangeRegex, CreateRange ),
            ( AboveRegex, CreateAbove ),
            ( BelowRegex, CreateBelow ),
        };

        /// <summary>パターン文字列から適切な Choice を生成する 一致しなければインデックス付き単一選択を返す</summary>
        public static IChoice CreateChoice(string text, int index)
        {
            foreach ((var regex, var method) in CreateList)
            {
                if (regex.IsMatch(text))
                    return method(text);
            }
            return new SingleValueChoice(text, index);
        }

        /// <summary>単一値選択パターンを解析して生成する</summary>
        private static SingleValueChoice CreateSingle(string text)
        {
            var match = SingleRegex.Match(text);
            if (match.Groups.Count <= 1)
                throw new InvalidDataException($"{text}を正常にパースできませんでした");

            var value = match.Groups[1].Value;
            text = TrimText(text, SingleRegex);
            return new SingleValueChoice(text, int.Parse(value));
        }

        /// <summary>範囲選択パターンを解析して生成する</summary>
        private static RangeValueChoice CreateRange(string text)
        {
            var match = RangeRegex.Match(text);
            if (match.Groups.Count <= 2)
                throw new InvalidDataException($"{text}を正常にパースできませんでした");

            var min = int.Parse(match.Groups[1].Value);
            var max = int.Parse(match.Groups[2].Value);
            text = TrimText(text, RangeRegex);
            return new RangeValueChoice(text, min, max);
            
        }

        /// <summary>下限以上パターンを解析して生成する</summary>
        private static AboveValueChoice CreateAbove(string text)
        {
            var match = AboveRegex.Match(text);
            if (match.Groups.Count <= 2) 
                throw new InvalidDataException($"{text}を正常にパースできませんでした");

            var value = match.Groups[1].Value;
            text = TrimText(text, AboveRegex);
            return new AboveValueChoice(text, int.Parse(value));
        }

        /// <summary>上限以下パターンを解析して生成する</summary>
        private static BelowValueChoice CreateBelow(string text)
        {
            var match = BelowRegex.Match(text);
            if (match.Groups.Count <= 2)
                throw new InvalidDataException($"{text}を正常にパースできませんでした");

            var value = match.Groups[2].Value;
            text = TrimText(text, BelowRegex);
            return new BelowValueChoice(text, int.Parse(value));
        }

        /// <summary>正規表現に一致した部分を除去しテキストを整形する</summary>
        private static string TrimText(string text, Regex regex)
        {
            return regex.Replace(text, string.Empty).Trim();
        }
    }
}
