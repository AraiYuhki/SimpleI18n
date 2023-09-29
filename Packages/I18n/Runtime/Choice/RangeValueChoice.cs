using System.IO;

namespace Xeon.Localization
{
    public class RangeValueChoice : IChoice
    {
        public string Text { get; }
        private readonly int _min;
        private readonly int _max;
        public RangeValueChoice(string text, int min, int max)
        {
            Text = text;
            _min = min;
            _max = max;
            Validate();
        }

        private void Validate()
        {
            if (_min >= _max)
            {
                throw new InvalidDataException("�ŏ��l���ő�l�𒴂��Ă��܂�");
            }
        }

        public bool IsMatch(int value)
            => _min <= value && value <= _max;
    }
}
