namespace VTT.IO
{
    /// <summary>
    /// Returns the correct IFileDialogService for the current platform.
    /// Unity Editor → EditorFileDialogService
    /// Build with USE_SFB defined → SFBFileDialogService
    /// Fallback → FallbackFileDialogService (writes to persistentDataPath)
    /// </summary>
    public static class FileDialogServiceFactory
    {
        public static IFileDialogService Create()
        {
#if UNITY_EDITOR
            return new EditorFileDialogService();
#elif USE_SFB
            return new SFBFileDialogService();
#else
            return new FallbackFileDialogService();
#endif
        }
    }
}
