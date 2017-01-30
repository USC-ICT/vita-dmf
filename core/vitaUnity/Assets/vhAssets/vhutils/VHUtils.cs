using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public static class VHUtils
{
    public static GameObject FindChild(GameObject root, string name)
    {
        // this function will search all active and inactive objects.  only searches one layer deep in the hierarchy.
        // you can however specify a 'path' to search in 'name'.  eg, FindChild(gun, "magazine/ammo");

        Transform child = root.transform.Find(name);
        return child != null ? child.gameObject : null;
    }

    public static GameObject FindChildRecursive(GameObject root, string name)
    {
        // this function will search all active and inactive objects.  does a recursive search through all child objects and their children
        // you cannot specify a 'path' in 'name'.  'name' must match the name of the object

        if (root.name == name)
            return root;

        for (int i = 0; i < root.transform.childCount; i++)
        {
            GameObject found = FindChildRecursive(root.transform.GetChild(i).gameObject, name);
            if (found != null)
                return found;
        }

        return null;
    }

    public static GameObject [] FindAllChildren(GameObject root)
    {
        // this function will return all active and inactive objects.  only returns one layer deep in the hierarchy

        List<GameObject> objects = new List<GameObject>();

        for (int i = 0; i < root.transform.childCount; i++)
        {
            objects.Add(root.transform.GetChild(i).gameObject);
        }

        return objects.ToArray();
    }

    public static GameObject [] FindAllChildrenRecursive(GameObject root)
    {
        // this function will return all active and inactive objects.  searches through all child objects and their children

        Stack<Transform> stack = new Stack<Transform>();
        List<GameObject> objects = new List<GameObject>();

        stack.Push(root.transform);

        while (stack.Count > 0)
        {
            Transform trans = stack.Pop();

            for (int i = 0; i < trans.childCount; i++)
            {
                Transform child = trans.GetChild(i);
                stack.Push(child);
                objects.Add(child.gameObject);
            }
        }

        return objects.ToArray();
    }

    public static T FindChildOfType<T>(GameObject root) where T : Component
    {
        // this function will search all active and inactive objects and active and inactive components.
        // This is a recursive search through all children.
        // it will return the first child it finds that matches the type

        GameObject [] children = FindAllChildrenRecursive(root);

        foreach (var child in children)
        {
            T [] components = child.GetComponents<T>();
            if (components.Length > 0)
                return components[0];
        }

        return default(T);
    }

    public static T [] FindAllChildrenOfType<T>(GameObject root) where T : Component
    {
        // this function will search all active and inactive objects and active and inactive components.
        // This is a recursive search through all children.
        // it will return all children it finds that matches the type.  Note that it returns script components. So if the same components is attached twice to a gameobject, it will return both of them.

        GameObject [] children = FindAllChildrenRecursive(root);
        List<T> childrenOfType = new List<T>();

        foreach (var child in children)
        {
            T [] components = child.GetComponents<T>();
            foreach (var component in components)
                childrenOfType.Add(component);
        }

        return childrenOfType.ToArray();
    }

    public static GameObject GetRootGameObject(GameObject child)
    {
        // this function will walk up the hierarchy until it finds a gameobject with a parent of 'null'.
        // it will return that gameobject

        Transform curr = child.transform;
        if (curr == null)
            return null;

        while (curr.parent != null)
        {
            curr = curr.parent;
        }

        return curr.gameObject;
    }

    public static void DestroyAllChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject.Destroy(parent.GetChild(i).gameObject);
        }
    }

    public static void DrawTransformLines(Transform t, float length)
    {
        Debug.DrawRay(t.position, t.right * length, Color.red);
        Debug.DrawRay(t.position, t.up * length, Color.green);
        Debug.DrawRay(t.position, t.forward * length, Color.blue);
    }


    public static void PlayWWWSound(MonoBehaviour behaviour, WWW www, AudioSource source, bool loop)
    {
        behaviour.StartCoroutine(PlayWWWSound(www, source, loop));
    }

    static IEnumerator PlayWWWSound(WWW www, AudioSource source, bool loop)
    {
        yield return www;

        source.clip = www.audioClip;
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
        while (!source.clip.isReadyToPlay)
#else
        while (www.audioClip.loadState == AudioDataLoadState.Unloaded ||
               www.audioClip.loadState == AudioDataLoadState.Loading)
#endif
        {
            yield return new WaitForEndOfFrame();
        }

        source.clip.name = www.url;
        source.loop = loop;
        source.Play();
    }


    public static void CreateAxisLines()
    {
        // this is a one-time function for generating the axis lines.  You can run this and copy-paste the objects into the scene
        float width = 0.01f;
        CreateCylinder(new Vector3(-10,0,0), new Vector3(0,0,0),  width, Color.red - new Color(0.5f,0,0));
        CreateCylinder(new Vector3(0,0,0),   new Vector3(10,0,0), width, Color.red);
        CreateCylinder(new Vector3(0,-10,0), new Vector3(0,0,0),  width, Color.green - new Color(0,0.5f,0));
        CreateCylinder(new Vector3(0,0,0),   new Vector3(0,10,0), width, Color.green);
        CreateCylinder(new Vector3(0,0,-10), new Vector3(0,0,0),  width, Color.blue - new Color(0,0,0.5f));
        CreateCylinder(new Vector3(0,0,0),   new Vector3(0,0,10), width, Color.blue);
    }

    public static void CreateCylinder( Vector3 start, Vector3 end, float width, Color color )
    {
        Vector3 offset = end - start;
        Vector3 scale = new Vector3(width, offset.magnitude / 2.0f, width);
        Vector3 position = start + (offset / 2.0f);

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);  //)Instantiate(cylinderPrefab, position, Quaternion.identity);
        cylinder.transform.position = position;
        cylinder.transform.rotation = Quaternion.identity;
        cylinder.transform.up = offset;
        cylinder.transform.localScale = scale;
        cylinder.GetComponent<Renderer>().material.color = color;
    }

    public static string GetCommonAspectText(float aspectRatio)
    {
       // http://en.wikipedia.org/wiki/List_of_common_resolutions
       const float check = 0.04f;
       if      (Math.Abs(aspectRatio - 1.00f) < check) return "1:1";
       else if (Math.Abs(aspectRatio - 1.25f) < check) return "5:4";
       else if (Math.Abs(aspectRatio - 1.33f) < check) return "4:3";
       else if (Math.Abs(aspectRatio - 1.50f) < check) return "3:2";
       else if (Math.Abs(aspectRatio - 1.60f) < check) return "16:10";
       else if (Math.Abs(aspectRatio - 1.66f) < check) return "5:3";
       else if (Math.Abs(aspectRatio - 1.77f) < check) return "16:9";

       // reverse
       else if (Math.Abs(aspectRatio - 0.80f) < check) return "4:5";
       else if (Math.Abs(aspectRatio - 0.75f) < check) return "3:4";
       else if (Math.Abs(aspectRatio - 0.66f) < check) return "2:3";
       else if (Math.Abs(aspectRatio - 0.62f) < check) return "10:16";
       else if (Math.Abs(aspectRatio - 0.60f) < check) return "3:5";
       else if (Math.Abs(aspectRatio - 0.56f) < check) return "9:16";

       else return "";
    }


    static public T DeserializeBytes<T>(byte[] bytes)
    {
        T binaryData = default(T);
        MemoryStream stream = new MemoryStream(bytes);
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();

            // Deserialize the hashtable from the file and
            // assign the reference to the local variable.
            binaryData = (T)formatter.Deserialize(stream);
        }
        catch (SerializationException e)
        {
            binaryData = default(T);
            Debug.LogError("Failed to deserialize. Reason: " + e.Message);
        }
        finally
        {
            stream.Close();
        }

        return binaryData;
    }


    static public void DisplayObject(GameObject obj, MonoBehaviour coroutineRunner, float displayTime, bool startsOn)
    {
        if (coroutineRunner != null)
        {
            obj.SetActive(startsOn);
            coroutineRunner.StartCoroutine(DisplayObjectCoroutine(obj, displayTime, startsOn));
        }
    }

    static IEnumerator DisplayObjectCoroutine(GameObject obj, float displayTime, bool startsOn)
    {
        yield return new WaitForSeconds(displayTime);
        obj.SetActive(!startsOn);
    }


    public static void ApplicationQuit()
    {
        if (Application.isEditor)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Play");
#endif
        }
        else
        {
            Application.Quit();
        }
    }

    public static void SceneManagerLoadScene(string sceneName)
    {
        // helper function for Application.LoadLevel(), for backward compatibility

#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
        Application.LoadLevel(sceneName);
#else
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
#endif
    }

    public static void SceneManagerLoadScene(int sceneBuildIndex)
    {
        // helper function for Application.LoadLevel(), for backward compatibility

#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
        Application.LoadLevel(sceneBuildIndex);
#else
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneBuildIndex);
#endif
    }

    public static AsyncOperation SceneManagerLoadSceneAsync(string sceneName)
    {
        // helper function for Application.LoadLevelAsync(), for backward compatibility

#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
        return Application.LoadLevelAsync(sceneName);
#else
        return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
#endif
    }

    public static string SceneManagerActiveSceneName()
    {
        // helper function for Application.loadedLevelName, for backward compatibility

#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
        return Application.loadedLevelName;
#else
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
#endif
    }

#if UNITY_EDITOR
    public static string EditorSceneManagerActiveSceneName()
    {
        // helper function for EditorApplication.currentScene, for backward compatibility

#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
        return UnityEditor.EditorApplication.currentScene;
#else
        return UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
#endif
    }
#endif


    /// <summary>
    /// used for obtaining a value that was passed in with an argument, i.e. -config toolkitConfig.ini
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static string GetCommandLineArgumentValue(string arg)
    {
        string argDash = "-" + arg;
        string [] arguments = GetCommandLineArgs(); // [0] is the name of the executable
        for (int i = 0; i < arguments.Length; i++)
        {
            if (arguments[i] == argDash)
            {
                if ((i + 1) < arguments.Length && !String.IsNullOrEmpty(arguments[i + 1]))
                    return arguments[i + 1];
            }
        }

        return null;
    }

    /// <summary>
    /// used for checking flag arguments i.e. nographics
    /// </summary>
    /// <param name="arg"></param>
    /// <returns>true if the argument flag was set</returns>
    public static bool HasCommandLineArgument(string arg)
    {
        string argDash = "-" + arg;
        return Array.Exists<string>(GetCommandLineArgs(), s => s == argDash);
    }


    public static string [] GetCommandLineArgs()
    {
        // iOS returns NULL, which we shouldn't have to check on each call

        if (IsIOS() || IsAndroid())
        {
            return new string[0];
        }
        else
        {
            return System.Environment.GetCommandLineArgs();
        }
    }


    public static bool IsUnity5OrGreater()
    {
        // this could probably be expanded to return a specific version number, etc.  but this fits our needs right now, since so many changes were done in unity 5
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
        return false;
#else
        return true;
#endif
    }


    public static bool Is64Bit()
    {
#if UNITY_64 || UNITY_EDITOR_64
        return true;
#else
        return false;
#endif
    }


    public static bool IsEditor()
    {
        return Application.isEditor;
    }


    [Obsolete("Web Player is deprecated as of Unity 5.3.  Please move away from Web Player specific code")]
    public static bool IsWebPlayer()
    {
        return Application.isWebPlayer;
    }


    public static bool IsWindows()
    {
        return Application.platform == RuntimePlatform.WindowsPlayer ||
               Application.platform == RuntimePlatform.WindowsEditor;
    }


    public static bool IsOSX()
    {
        return Application.platform == RuntimePlatform.OSXPlayer ||
               Application.platform == RuntimePlatform.OSXEditor;
    }


    public static bool IsWindows8OrGreater()
    {
        // win10 has the same version number unless the app has been 'manifested for Win10'
        // links, in case we ever care:
        // https://msdn.microsoft.com/library/windows/desktop/ms724832.aspx
        // https://msdn.microsoft.com/en-us/library/windows/desktop/dn481241.aspx
        // https://msdn.microsoft.com/library/windows/desktop/ms724451(v=vs.85).aspx

        // we use this for deciding which TTS voice to use.  This could be enhanced to return a version class, and then you could compare it against const's for different windows versions, etc.

        Version win8version = new Version(6, 2, 9200, 0);
        return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= win8version;
    }


    public static bool IsWindows10OrGreater()
    {
        // see notes above for IsWindows8OrGreater()

        Version win10version = new Version(10, 0, 0, 0);
        return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= win10version;
    }


    public static bool IsIOS()
    {
        if (IsEditor())
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS;
#endif
        }

        return Application.platform == RuntimePlatform.IPhonePlayer;
    }


    public static bool IsAndroid()
    {
        if (IsEditor())
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android;
#endif
        }

        return Application.platform == RuntimePlatform.Android;
    }


    public static bool IsWebGL()
    {
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
        return false;
#else
        if (IsEditor())
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL;
#endif
        }

        return Application.platform == RuntimePlatform.WebGLPlayer;
#endif
    }


    public static void DeleteChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            GameObject.Destroy(child.gameObject);
        }
    }


    public static string GetSVNRevision(string path)
    {
        // this function will return the output from svnversion if the given path is a svn working folder.
        // eg:
        // string version = GetSVNRevision(Application.dataPath + "/../../../");
        // version will contain one of the following:
        // 4123:4168     mixed revision working copy
        // 4168M         modified working copy
        // 4123S         switched working copy
        // 4123P         partial working copy, from a sparse checkout
        // 4123:4168MS   mixed revision, modified, switched working copy
        //
        // if folder is not a svn working copy, it will return "" empty string
        // this function will only run on windows, if run on any other platform, it will return "" empty string

        string versionText = "";

        // check to see if folder is a svn working copy
        if (VHUtils.IsWindows() || VHUtils.IsOSX())
            return versionText;

        try
        {
            if (!Directory.Exists(path))
                return versionText;

            // run 'svnversion' on the folder
            System.Diagnostics.Process svn = new System.Diagnostics.Process {
                StartInfo = new System.Diagnostics.ProcessStartInfo {
                    FileName = "svnversion",
                    Arguments = "-n " + path,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            string output = "";

            svn.Start();
            while (!svn.StandardOutput.EndOfStream)
            {
                output += svn.StandardOutput.ReadLine();
            }
            svn.WaitForExit();

            // svnversion .
            // 4123:4168     mixed revision working copy
            // 4168M         modified working copy
            // 4123S         switched working copy
            // 4123P         partial working copy, from a sparse checkout
            // 4123:4168MS   mixed revision, modified, switched working copy
            // Unversioned directory

            if (!output.Contains("Unversioned"))
            {
                versionText = output;
            }
        }
        catch (Exception)
        {
            // if svnversion isn't in the path, or any other error we encounter
        }

        return versionText;
    }


#if !UNITY_WEBGL
    public static void RPC(NetworkView source, string name, NetworkPlayer target, params object [] args)
    {
        // disable Deprecation warnings until proper workaround can be implemented
#pragma warning disable 612, 618
        source.RPC(name, target, args);
#pragma warning restore 612, 618
    }

    public static void RPC(NetworkView source, string name, RPCMode mode, params object [] args)
    {
        // disable Deprecation warnings until proper workaround can be implemented
#pragma warning disable 612, 618
        source.RPC(name, mode, args);
#pragma warning restore 612, 618
    }
#endif
}
