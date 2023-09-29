namespace Xeon.Localization
{
    public class BelowValueChoice : IChoice
    {
        public string Text { get; }
        private readonly int _max;
        public BelowValueChoice(string text, int max)
        {
            Text = text;
            _max = max;
        }

        public bool IsMatch(int value)
            => value <= _max;
    }
}
