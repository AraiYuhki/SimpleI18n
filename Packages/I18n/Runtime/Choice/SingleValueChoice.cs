namespace Xeon.Localization
{
    public class SingleValueChoice : IChoice
    {
        public string Text { get; }

        private readonly int _value;
        public SingleValueChoice(string text, int value)
        {
            Text = text;
            _value = value;
        }

        public bool IsMatch(int value)
            => _value == value;
    }
}
