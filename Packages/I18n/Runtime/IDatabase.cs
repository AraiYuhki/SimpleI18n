namespace Xeon.Localization
{
    public interface IDatabase
    {
        ITranslateDataSet FindByKey(string key);
        bool TryFindByKey(string key, out ITranslateDataSet translated);
    }
}
