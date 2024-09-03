namespace Xeon.Localization
{
    public interface IChoice
    {
        string Text { get; }
        bool IsMatch(int value);
    }
}
