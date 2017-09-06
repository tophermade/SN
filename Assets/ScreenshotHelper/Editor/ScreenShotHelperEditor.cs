using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;
using System.Xml.Serialization;

[CustomEditor(typeof(ScreenshotHelper))]
public class ScreenShotHelperEditor : Editor
{
    private static List<bool> foldoutState = new List<bool>();


    void OnEnable()
    {
        ScreenshotHelper myTarget = (ScreenshotHelper)target;

        bool loadConfig = false;
        if (myTarget.shotInfo.Count == 0 || !string.IsNullOrEmpty(myTarget.configFile))
        {
            
            if (!string.IsNullOrEmpty(myTarget.configFile))
            {
                if (File.Exists(myTarget.configFile))
                    loadConfig = true;
            }

            if (loadConfig)
            {
                LoadPresetFile(myTarget.configFile, myTarget);
            }
            else
            {
                myTarget.SetDefaults();
            }
        }

        if (!loadConfig)
        {
            LoadPresetFile(SSHPreset.DefaultSavePath(), myTarget);
            if (File.Exists(SSHPreset.DefaultSavePath()))
            {
                File.Delete(SSHPreset.DefaultSavePath());
            }
        }

        myTarget.PathChange = PathChange;
        //myTarget.DefaultsSet = SaveSettings;
    }


    private void PathChange(string newPath)
    {
        Debug.Log("PathChange to " + newPath);
        ScreenshotHelper myTarget = (ScreenshotHelper)target;
        myTarget.savePath = newPath;
        SSHPreset sshPreset = new SSHPreset();
        sshPreset.Save(myTarget);
    }


    void OnApplicationQuit()
    {
        ScreenshotHelper myTarget = (ScreenshotHelper)target;
        SSHPreset sshPreset = new SSHPreset();
        sshPreset.Save(myTarget);
    }

    public override void OnInspectorGUI()
    {
        ScreenshotHelper myTarget = (ScreenshotHelper)target;

        RenderTextureToggleAndWarning(myTarget);

        myTarget.SSHTextureFormat = (ScreenshotHelper.tSSHTextureFormat)EditorGUILayout.EnumPopup("Texture Format", myTarget.SSHTextureFormat);

        CameraSolidColorTransparencyWarning(myTarget);

        MakeSpace(1);

        EditorGUI.BeginChangeCheck();

        myTarget.keyToHold = (KeyCode)EditorGUILayout.EnumPopup("Key to hold:", myTarget.keyToHold);
        myTarget.keyToPress = (KeyCode)EditorGUILayout.EnumPopup("Key to press:", myTarget.keyToPress);
        if (myTarget.keyToPress == KeyCode.None)
        {
            EditorGUILayout.HelpBox("If you don't assign a key to press then you will not be able to take screenshots with a key press.", MessageType.Warning, true);
        }

        MakeSpace(1);
        myTarget.orientation = (SSHOrientation)EditorGUILayout.EnumPopup("Orientation", myTarget.orientation);

        if (EditorGUI.EndChangeCheck())
            myTarget.UpdateDimensions();

        //sizes header
        MakeSpace(1);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Screen Shot Sizes", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.HelpBox("Expand each to edit", MessageType.None, true);
        EditorGUILayout.EndHorizontal();
        MakeSpace(1);

        SetSizesSubs(myTarget);

        //add a size
        EditorGUILayout.Space();
        if (GUILayout.Button("Add a size"))
        {
            myTarget.shotInfo.Add(new ShotSize(0, 0));
        }


        MakeSpace(3);

        EditorGUILayout.HelpBox("In-editor Save location: " + myTarget.savePath, MessageType.None);
        if (GUILayout.Button("Set in-editor save location"))
        {
            myTarget.savePath = GameViewUtils.SelectFileFolder(Directory.GetCurrentDirectory(), "");
            PathChange(myTarget.savePath);
        }

        MakeSpace(1);

        EditorGUILayout.HelpBox("Build Save location example: " + myTarget.BuildSaveLocation(), MessageType.None);
        if (GUILayout.Button("Set build save location"))
        {
            ScreenshotHelperBuildSaveLocationWindow.ShowWindow(myTarget);
        }


        MakeSpace(2);

        if (GUILayout.Button("Save Presets"))
        {
            string newConfig = SavePreset(myTarget);
            if (!string.IsNullOrEmpty(newConfig))
            {
                myTarget.configFile = newConfig;
            }
           
        }

        LoadPresetsButton(myTarget);

        MakeSpace(2);

        if (GUILayout.Button("Load Defaults"))
        {
            myTarget.SetDefaults();
            myTarget.configFile = "";
        }

        MakeSpace(1);
    }

    private void MakeSpace(int numSpaces)
    {
        for (int i = 0; i < numSpaces; i++)
        {
            EditorGUILayout.Space();
        }
    }

    private void RenderTextureToggleAndWarning(ScreenshotHelper myTarget)
    {
        myTarget.useRenderTexture = EditorGUILayout.ToggleLeft("Use Render to Texture", myTarget.useRenderTexture);
        string rtMessage = "Render to Texture requires Unity Pro or Unity 5 (or newer). \nImages will be scaled with a bilinear scaling method.";
        string message = "Render to Texture provides the best possible resolution.";
        MessageType messageType = MessageType.Info;

        if (myTarget.useRenderTexture)
        {
            string canvasWarning = GetIsCanvasesValidMessage();

            if (!GetIsRenderTextureAvailable())
            {
                message = rtMessage;
            }
            else if (!string.IsNullOrEmpty(canvasWarning))
            {
                message = canvasWarning;
            }

        }

        if (myTarget.useRenderTexture && !GetIsRenderTextureAvailable())
        {
            myTarget.useRenderTexture = false;

        }

        if (!myTarget.useRenderTexture && GetIsRenderTextureAvailable())
        {
            message = "Use Render to texture for the best possible resolutions";
        }

        if (!myTarget.useRenderTexture && !GetIsRenderTextureAvailable())
            message = rtMessage;

        if (!string.IsNullOrEmpty(message))
            EditorGUILayout.HelpBox(message, messageType, true);
    }

    private void SetSizesSubs(ScreenshotHelper myTarget)
    {
        for (int i = 0; i < myTarget.shotInfo.Count; i++)
        {
            if (foldoutState.Count < i + 1)
            {
                foldoutState.Add(false);
            }

            string fileName = myTarget.GetScreenShotName(myTarget.shotInfo[i]);

            foldoutState[i] = EditorGUILayout.Foldout(foldoutState[i], fileName);
            if (foldoutState[i])
            {
                EditorGUILayout.BeginHorizontal();
                myTarget.shotInfo[i].width = EditorGUILayout.IntField("\tWidth: ", myTarget.shotInfo[i].width);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                myTarget.shotInfo[i].height = EditorGUILayout.IntField("\tHeight: ", myTarget.shotInfo[i].height);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                myTarget.shotInfo[i].label = EditorGUILayout.TextField("\tPrefix: ", myTarget.shotInfo[i].label);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("\t");
                if (GUILayout.Button("Remove" , GUILayout.Width(100)))
                {
                    int index = i;
                    myTarget.shotInfo.Remove(myTarget.shotInfo[index]);
                    foldoutState.Remove(foldoutState[index]);
                }
                EditorGUILayout.EndHorizontal();
            }
           
        }
    }
    
    public bool GetIsRenderTextureAvailable()
    {
        string unityVersion = UnityEditorInternal.InternalEditorUtility.GetFullUnityVersion();
        string[] major = unityVersion.Split('.');

        if (Convert.ToInt32(major[0]) < 5 && !UnityEditorInternal.InternalEditorUtility.HasPro())
        {
            return false;
        }
        return true;
    }

    public string GetIsCanvasesValidMessage()
    {
        Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
        string dOut = "";
        dOut = "Canvases need to use ScreenSpaceCamera or Worldspace to properly render to texture and must have the main camera attached.\n";
        bool error = false;
        foreach (Canvas c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                error = true;
                dOut += "\nCanvas '" + c.gameObject.name + "' is in Screen Space Overlay mode and will not render to texture.";
            }

            if (c.worldCamera != Camera.main)
            {
                error = true;
                dOut += "\nCanvas '" + c.gameObject.name + "' does not have the main camera attached and cannot render to texture.";
            }
        }

        if (error)
            return dOut;

        return null;
    }

    private void CameraSolidColorTransparencyWarning(ScreenshotHelper myTarget)
    {
        if (myTarget.useRenderTexture && isAlphaFormat(myTarget.SSHTextureFormat))
        {
            if (Camera.main.clearFlags == CameraClearFlags.Color ||
                Camera.main.clearFlags == CameraClearFlags.SolidColor)
            {
                if (Camera.main.backgroundColor.a < 1.0)
                {
                    EditorGUILayout.HelpBox("Main camera is using solid color with alpha < 1. This will result in images with transparent backgrounds.", MessageType.Warning, true);
                }
            }
        }
    }

    private void LoadPresetsButton(ScreenshotHelper ssh)
    {
        if (GUILayout.Button("Load Presets"))
        {
            string newConfig = EditorUtility.OpenFilePanel("Select a preset file", Directory.GetCurrentDirectory(), "xml");
            if (!string.IsNullOrEmpty(newConfig))
            {
                if (LoadPresetFile(newConfig, ssh))
                {
                    ssh.configFile = newConfig;
                }
            }
        }
    }

    private bool LoadPresetFile(string fileName , ScreenshotHelper ssh)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        if (!File.Exists(fileName))
            return false;

        SSHPreset sshPreset = SSHPreset.Load(fileName);
        if (sshPreset != null)
        {
            if (sshPreset.sizes.Count > 0)
            {
                ssh.shotInfo = sshPreset.sizes;
                ssh.savePath = sshPreset.lastSavePath;
                ssh.orientation = sshPreset.orientation;
                return true;
            }
        }

        return false;
    }

    private string SavePreset(ScreenshotHelper ssh)
    {
        string file = EditorUtility.SaveFilePanel("Save your presets", Directory.GetCurrentDirectory(), "screenshot_helper_preset", "xml");

        SSHPreset preset = new SSHPreset();
        preset.Save(file, ssh);

        return file;
    }

    private void SortSizes()
    {
        ScreenshotHelper myTarget = (ScreenshotHelper)target;
        List<ShotSize> shotSizes = myTarget.shotInfo;
        List<string> fileNames = new List<string>();
        for (int i = 0; i < shotSizes.Count; i++)
        {
            fileNames.Add(myTarget.GetScreenShotName(shotSizes[i]));
        }

        fileNames.Sort();
        ShotSize[] tempShotSizes = new ShotSize[shotSizes.Count];

        for (int i = 0; i < fileNames.Count; i++)
        {
            for (int j = 0; j < shotSizes.Count; j++)
            {
                if (myTarget.GetScreenShotName(shotSizes[j]) == fileNames[i])
                    tempShotSizes[i] = shotSizes[j];
            }
        }

        myTarget.shotInfo = new List<ShotSize>();
        for (int i = 0; i < tempShotSizes.Length; i++)
        {
            myTarget.shotInfo.Add(tempShotSizes[i]);
        }
    }

    private bool isAlphaFormat(ScreenshotHelper.tSSHTextureFormat textureFormat)
    {
        string tf = textureFormat.ToString().ToLower();
        if (tf.Contains("a"))
            return true;

        return false;
    }
}

[XmlRoot("SSHPreset")]
public class SSHPreset
{
    public SSHOrientation orientation = SSHOrientation.portrait;
    public string lastSavePath = "";
    public ScreenshotHelper.tSSHTextureFormat textureFormat = ScreenshotHelper.tSSHTextureFormat.RGB24;
    public KeyCode keyToPress = KeyCode.S;
    public KeyCode keyToHold = KeyCode.LeftShift;
    public Environment.SpecialFolder buildPathRoot;
    public string buildPathExtra;

    [XmlArray("sizes")]
    [XmlArrayItem("size")]
    public List<ShotSize> sizes = new List<ShotSize>();

    public void Save(ScreenshotHelper ssh)
    {
        string fileName = DefaultSavePath();
        Save(fileName, ssh);
    }

    public void Save(string fileName , ScreenshotHelper ssh)
    {
        if (string.IsNullOrEmpty(fileName))
            return;
        if (ssh.shotInfo.Count <= 0)
        {
            var tempDelegate = ssh.DefaultsSet;
            ssh.DefaultsSet = null;
            ssh.SetDefaults();
            ssh.DefaultsSet = tempDelegate;
        }

        sizes = ssh.shotInfo;
        orientation = ssh.orientation;
        lastSavePath = ssh.savePath;
        keyToPress = ssh.keyToPress;
        keyToHold = ssh.keyToHold;
        buildPathRoot = ssh.buildSavePathRoot;
        buildPathExtra = ssh.buildSavePathExtra;
        textureFormat = ssh.SSHTextureFormat;

        var serializer = new XmlSerializer(typeof(SSHPreset));
        using (var stream = new FileStream(fileName, FileMode.Create))
            serializer.Serialize(stream , this);
    }

    public static SSHPreset Load(string fileName)
    {
        if (File.Exists(fileName))
        {
            var serializer = new XmlSerializer(typeof(SSHPreset));
            using (var stream = new FileStream(fileName , FileMode.Open))
                return serializer.Deserialize(stream) as SSHPreset;
        }

        return null;
    }

    public static string DefaultSavePath()
    {
        string path = Application.persistentDataPath + @"/sshdefaults.xml";
        return path;
    }
}


