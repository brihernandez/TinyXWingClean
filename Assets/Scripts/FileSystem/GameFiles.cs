using UnityEngine;
using System.IO;

public static class GameFiles
{
    private const string ShipsRelativePath = "/Ships";

    public static string GameDataFolder =>
        GetFolderSafe(Application.streamingAssetsPath);

    public static string ShipsFolder =>
        GetFolderSafe(GameDataFolder + ShipsRelativePath);

    private static string GetFolderSafe(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        return folderPath;
    }
}