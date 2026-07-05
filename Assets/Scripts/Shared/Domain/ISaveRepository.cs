namespace Shared.Domain
{
    public interface ISaveRepository
    {
        void Save(string fileName, string content);
        string Load(string fileName);
        bool Exists(string fileName);
    }
}