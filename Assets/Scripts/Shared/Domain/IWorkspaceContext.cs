namespace Shared.Domain
{
    public interface IWorkspaceContext
    {
        string CurrentId { get; set; }
        string GetWorkspacePath();
        string GetRelativePath(string absolutePath);
    }
}