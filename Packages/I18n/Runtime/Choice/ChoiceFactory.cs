using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Xeon.Localization
{
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

        public static IChoice CreateChoice(string text, int index)
        {
            foreach ((var regex, var method) in CreateList)
            {
                if (regex.IsMatch(text))
                    return method(text);
            }
            return new SingleValueChoice(text, index);
        }

        private static SingleValueChoice CreateSingle(string text)
        {
            var match = SingleRegex.Match(text);
            if (match.Groups.Count <= 1)
                throw new InvalidDataException($"{text}を正常にパースできませんでした");

            var value = match.Groups[1].Value;
            text = TrimText(text, SingleRegex);
            return new SingleValueChoice(text, int.Parse(value));
        }

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

        private static AboveValueChoice CreateAbove(string text)
        {
            var match = AboveRegex.Match(text);
            if (match.Groups.Count <= 2) 
                throw new InvalidDataException($"{text}を正常にパースできませんでした");

            var value = match.Groups[1].Value;
            text = TrimText(text, AboveRegex);
            return new AboveValueChoice(text, int.Parse(value));
        }

        private static BelowValueChoice CreateBelow(string text)
        {
            var match = BelowRegex.Match(text);
            if (match.Groups.Count <= 2)
                throw new InvalidDataException($"{text}を正常にパースできませんでした");

            var value = match.Groups[2].Value;
            text = TrimText(text, BelowRegex);
            return new BelowValueChoice(text, int.Parse(value));
        }

        private static string TrimText(string text, Regex regex)
        {
            return regex.Replace(text, string.Empty).Trim();
        }
    }
}
