using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;


public class PostProcessBuild
{
    // https://docs.unity3d.com/ScriptReference/Callbacks.PostProcessBuildAttribute.html
    [PostProcessBuildAttribute]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.LogFormat("OnPostprocessBuild() - {0} - {1}", target, pathToBuiltProject);

        if (target == BuildTarget.StandaloneWindows ||
            target == BuildTarget.StandaloneWindows64)
        {
            string destFolder = Path.GetDirectoryName(pathToBuiltProject);

            // move folders one folder deep to hide .exe
            string dataFolder = Path.Combine(destFolder, "vita_Data");
            string exeFile = Path.Combine(destFolder, "vita.exe");
            string newDataFolder = Path.Combine(destFolder, "data");

            if (Directory.Exists(newDataFolder))
                Directory.Delete(newDataFolder, true);

            Directory.CreateDirectory(newDataFolder);
            Directory.Move(dataFolder, Path.Combine(newDataFolder, "vita_Data"));
            File.Move(exeFile, Path.Combine(newDataFolder, "vita.exe"));

            // copy .bat files to top-level folder
            CopyFile(destFolder, "start.exe");
            CopyFile(destFolder, "start-singlescreen.exe");
            CopyFile(destFolder, "start-splitscreen.exe");
            CopyFile(destFolder, "VITA_AR_User_Guide.pdf");
        }
    }

    static void CopyFile(string destFolder, string name)
    {
        // special purpose function specifically for these files

        string sourceFile = Path.Combine(Application.dataPath, @"Editor/" + name);
        string destFile = Path.Combine(destFolder, name);

        Debug.LogFormat("OnPostprocessBuild() - Copying '{0}' to '{1}'", sourceFile, destFile);

        File.Copy(sourceFile, destFile, true);
    }
}
