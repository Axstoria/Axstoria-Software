using System.IO;
using Shared.Domain;
using UnityEngine;

namespace Shared.Infrastructure.Shared.Infrastructure
{
    public class WorkspaceContext : IWorkspaceContext
    {
        public string CurrentId { get; set; }

        public string GetWorkspacePath()
        {
            if (string.IsNullOrEmpty(CurrentId)) return null;
            return Path.Combine(Application.persistentDataPath, "Workspace", CurrentId);
        }

        public string GetRelativePath(string absolutePath)
        {
            return absolutePath.Replace(GetWorkspacePath() + Path.DirectorySeparatorChar, "");
        }
    }
}