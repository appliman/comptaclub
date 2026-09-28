namespace ComptaClub.Configuration;

public static class SettingsHelpers
{
    public static string CreateFolderIfNotExists(string folder)
    {
        var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
        var currentFolder = Path.GetDirectoryName(entryAssembly!.Location)!;

        string? relativeRootPath = null;
        if (folder.StartsWith("./")
            || folder.StartsWith(@".\"))
        {
            relativeRootPath = folder.Substring(0,2);
            var folderPath = folder.Replace(relativeRootPath[1], System.IO.Path.DirectorySeparatorChar)
                .TrimStart('.')
                .TrimStart(System.IO.Path.DirectorySeparatorChar);

            folder = Path.Combine(currentFolder, folderPath);
        }

        Directory.CreateDirectory(folder);
        return folder;
    }
}
