namespace Xeon.Localization
{
    public class AboveValueChoice : IChoice
    {
        public string Text { get; }
        private readonly int _min;
        public AboveValueChoice(string text, int min)
        {
            Text = text;
            _min = min;
        }

        public bool IsMatch(int value)
            => _min <= value;
    }
}
