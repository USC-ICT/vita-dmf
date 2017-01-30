using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class OnGeneratedCSProjectFilesMonoDevelop : AssetPostprocessor
{
    static void OnGeneratedCSProjectFiles()
    {
        // This function modifies the generated .sln file
        // For MonoDevelop, this function changes the default style policy to match the coding style used in our projects.
        // The code block added below is the result of doing this in MonoDevelop:
        // Project->Solution Options->Source Code->Code Formatting->Text File.  Convert tabs to spaces checkbox.

        //Debug.LogFormat("OnGeneratedCSProjectFilesMonoDevelop.OnGeneratedCSProjectFiles() - {0}", DateTime.Now);

        string projectDirectory = System.IO.Directory.GetParent(Application.dataPath).FullName;
        string sln = Path.Combine(projectDirectory, Path.GetFileNameWithoutExtension(projectDirectory) + ".sln");

        string find = @"StartupItem = Assembly-CSharp.csproj";
        string replace = 
            @"		Policies = $0" + "\n" +
            @"		$0.DotNetNamingPolicy = $1" + "\n" +
            @"		$1.DirectoryNamespaceAssociation = None" + "\n" +
            @"		$1.ResourceNamePolicy = FileFormatDefault" + "\n" +
            @"		$0.TextStylePolicy = $2" + "\n" +
            @"		$2.inheritsSet = null" + "\n" +
            @"		$2.scope = text/x-csharp" + "\n" +
            @"		$0.CSharpFormattingPolicy = $3" + "\n" +
            @"		$3.AfterDelegateDeclarationParameterComma = True" + "\n" +
            @"		$3.inheritsSet = Mono" + "\n" +
            @"		$3.inheritsScope = text/x-csharp" + "\n" +
            @"		$3.scope = text/x-csharp" + "\n" +
            @"		$0.TextStylePolicy = $4" + "\n" +
            @"		$4.FileWidth = 120" + "\n" +
            @"		$4.inheritsSet = VisualStudio" + "\n" +
            @"		$4.inheritsScope = text/plain" + "\n" +
            @"		$4.scope = text/plain";

        if (File.Exists(sln))
        {
            try
            {
                string text = File.ReadAllText(sln);
                if (text.IndexOf(find) != -1)
                {
                    text = text.Replace(find, find + Environment.NewLine + replace);
                    File.WriteAllText(sln, text);
                }
            }
            catch (Exception e)
            {
                Debug.LogFormat("OnGeneratedCSProjectFilesMonoDevelop.OnGeneratedCSProjectFiles() - The file could not be read: {0}", e.Message);
            }
        }
        else
        {
            Debug.LogError("OnGeneratedCSProjectFiles failed. No sln named: " + sln);
        }
    }
}
