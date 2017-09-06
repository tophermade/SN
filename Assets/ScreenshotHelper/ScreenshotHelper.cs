using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Xml.Serialization;

public class ScreenshotHelper : MonoBehaviour
{
    public delegate void ScreenChange();
    public ScreenChange OnScreenChanged;

    public bool useRenderTexture = true;
    public enum tSSHTextureFormat { ARGB32, RGB24, RGBAFloat, RGBAHalf }
    public tSSHTextureFormat SSHTextureFormat = tSSHTextureFormat.RGB24;

    private TextureFormat textureFormat
    {
        get
        {
            switch (SSHTextureFormat)
            {
                case tSSHTextureFormat.ARGB32:
                    return TextureFormat.ARGB32;
                case tSSHTextureFormat.RGBAFloat:
                    return TextureFormat.RGBAFloat;
                case tSSHTextureFormat.RGBAHalf:
                    return TextureFormat.RGBAHalf;
                case tSSHTextureFormat.RGB24:
                default:
                    return TextureFormat.RGB24;
            }
        }
    }

    private int Depth
    {
        get
        {
            switch (SSHTextureFormat)
            {
                case tSSHTextureFormat.ARGB32:
                    return 32;
                case tSSHTextureFormat.RGBAFloat:
                    return 32;
                case tSSHTextureFormat.RGBAHalf:
                    return 16;
                case tSSHTextureFormat.RGB24:
                default:
                    return 24;
            }
        }
    }

    public SSHOrientation orientation = SSHOrientation.portrait;
    public List<ShotSize> shotInfo = new List<ShotSize>();  

    #pragma warning disable 0414
    private ShotSize maxRes;
    #pragma warning restore 0414
    
    public string savePath = "";
    public Environment.SpecialFolder buildSavePathRoot = Environment.SpecialFolder.MyPictures;
    public string buildSavePathExtra = "screenshots";
    public string configFile = "";

    public KeyCode keyToHold = KeyCode.LeftShift;
    public KeyCode keyToPress = KeyCode.S;

    private static ScreenshotHelper _instance;
    public static ScreenshotHelper instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<ScreenshotHelper>();
                if (Application.isPlaying)
                    DontDestroyOnLoad(_instance.gameObject);    
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != _instance)
            {
                Destroy(this.gameObject);
            }
        }

        /*
    #if UNITY_EDITOR
            if (string.IsNullOrEmpty(savePath))
                savePath = System.IO.Directory.GetCurrentDirectory() + "/screenshothelper";
                */

#if !UNITY_EDITOR
        savePath = BuildSaveLocation();
#endif


        maxRes = new ShotSize(Screen.currentResolution.width, Screen.currentResolution.height);
    }

    public string BuildSaveLocation()
    {
        return Environment.GetFolderPath(buildSavePathRoot) + "/" + buildSavePathExtra;
    }

   
    void Update()
    {
        if (keyToHold == KeyCode.None && keyToPress != KeyCode.None)
        {
            if (Input.GetKeyDown(keyToPress))
                StartCoroutine(TakeScreenShots()); 
        }
        else if (keyToHold != KeyCode.None && keyToPress != KeyCode.None)
        {
            if (Input.GetKey(keyToHold) && Input.GetKeyDown(keyToPress))
                StartCoroutine(TakeScreenShots());
        }
    }


    public void UpdateDimensions()
    {
        for (int i = 0; i < shotInfo.Count; i++)
        {
            if (orientation == SSHOrientation.landscape &&
                shotInfo[i].height > shotInfo[i].width)
            {
                int temp = shotInfo[i].width;
                shotInfo[i].width = shotInfo[i].height;
                shotInfo[i].height = temp;
            }
            else if (orientation == SSHOrientation.portrait &&
                shotInfo[i].width > shotInfo[i].height)
            {
                int temp = shotInfo[i].width;
                shotInfo[i].width = shotInfo[i].height;
                shotInfo[i].height = temp;
            }
        }
    }

    private void WarnCanvases()
    {
        Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
        List<string> dOut = new List<string>();
        dOut.Add("Canvases need to use ScreenSpaceCamera or Worldspace to properly render to texture and must have the main camera attached.");
        bool error = false;
        foreach (Canvas c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                error = true;
                dOut.Add("Canvas " + c.gameObject.name + " is in Screen Space Overlay mode and will not render to texture.");
            }

            if (c.worldCamera != Camera.main)
            {
                error = true;
                dOut.Add("Canvas " + c.gameObject.name + " does not have the main camera attached and cannot render to texture.");
            }
        }

        if (error)
        {
            foreach (string s in dOut)
            {
                SSHDebug.LogWarning(s);
            }
        }
    }

    public void GetScreenShots()
    {
        StartCoroutine(TakeScreenShots());
    }


    public int GetFileNum(string typeExt)
    {
        if (!Directory.Exists(savePath))
            return 0;

        string[] files = Directory.GetFiles(savePath, "*" + typeExt + "*");

        int min = -1;
        foreach (var fileName in files)
        {
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string[] splitString = baseName.Split('_');

            if (splitString.Length > 0)
            {
                int thisNum = Convert.ToInt32(splitString[splitString.Length - 1]);

                if (thisNum > min)
                    min = thisNum;
            }
        }


        return min + 1;
    }

    public delegate void PathChangeDelegate(string newPath);
    public PathChangeDelegate PathChange;

    bool isTakingShots = false;
    IEnumerator TakeScreenShots()
    {        
        #if UNITY_EDITOR
        if (!Directory.Exists(savePath))
        {
            string newPath = GameViewUtils.SelectFileFolder(System.IO.Directory.GetCurrentDirectory(), "");
            if (!string.IsNullOrEmpty(newPath))
            {
                savePath = newPath;
                if (PathChange != null)
                    PathChange(newPath);
            }
        }
        #endif


        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        float timeScaleStart = Time.timeScale;
        Time.timeScale = 0f;

        #pragma warning disable 0219
        ShotSize initialRes = new ShotSize(Screen.width, Screen.height);
        #pragma warning restore 0219

        if (isTakingShots)
        {
            yield break;
        }

        isTakingShots = true;
        string fileName = "";
        
        #if UNITY_EDITOR
        int currentIndex = GameViewUtils.GetCurrentSizeIndex();
        //Maximize game view seems to cause crashes 
        //GameViewUtils.MaximizeGameView(true);
        #endif

        foreach (var shot in shotInfo)
        {
            fileName = GetScreenShotName(shot);

            

            #if UNITY_EDITOR
            GameViewUtils.SetSize(shot.width, shot.height);
            if (OnScreenChanged != null)
            {
                yield return new WaitForEndOfFrame();
                OnScreenChanged();
                yield return new WaitForEndOfFrame();
            }
            Canvas.ForceUpdateCanvases();
            #else

            float ratio = (1f * shot.width) / (1f * shot.height);
            SSH_IntVector2 thisRes = new SSH_IntVector2(shot.width , shot.height);
            

            if (shot.height > maxRes.height)
            {
                thisRes.width = Mathf.FloorToInt(maxRes.height * ratio);
                thisRes.height = maxRes.height;
            }

            Screen.SetResolution(thisRes.width, thisRes.height, false);
            Canvas.ForceUpdateCanvases();
            yield return new WaitForEndOfFrame();

            int attempts = 0;
            while (Screen.width != thisRes.width && attempts < 10)
            {
                Screen.SetResolution(thisRes.width, thisRes.height, false);
                Canvas.ForceUpdateCanvases();
                yield return new WaitForEndOfFrame();
                attempts++;
            }
            
            #endif

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            Texture2D tex = null;
            if (useRenderTexture)
            {
                RenderTexture rt = new RenderTexture(shot.width, shot.height, Depth);
                Camera.main.targetTexture = rt;
                tex = new Texture2D(shot.width, shot.height, TextureFormat.RGB24, false);
                Camera.main.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, shot.width, shot.height), 0, 0);
                Camera.main.targetTexture = null;
                RenderTexture.active = null;
                Destroy(rt);
            }
            else
            {
                tex = new Texture2D(Screen.width, Screen.height, textureFormat, false);
                Vector2 camUpperLeft = Camera.main.pixelRect.min; //lower left
                camUpperLeft.y = Camera.main.pixelRect.min.y;

                float offsetX = 0f; //0.5f * Screen.width;
                float offsetY = 0f; //0.5f * Screen.height;
#if UNITY_EDITOR
                //Vector2 gameViewSize = GameViewUtils.GetMainGameViewSize();
                //offsetX = 338f;
                //offsetY = gameViewSize.y;
#endif
                // Was in while there was an issue in Unity when taking screenshots from editor.
                //Debug.LogFormat("screen: ({0},{1}) -- shot: ({2},{3}) -- offset: ({4},{5})", Screen.width, Screen.height, shot.width, shot.height, offsetX, offsetY);
                tex.ReadPixels(new Rect(offsetX, offsetY, Screen.width, Screen.height), 0, 0);
                tex.Apply();
                TextureScale.Bilinear(tex, shot.width, shot.height);
            }

            if (tex != null)
            {
                byte[] screenshot = tex.EncodeToPNG();
                var file = File.Open(Path.Combine(savePath, fileName), FileMode.Create);
                var binaryWriter = new BinaryWriter(file);
                binaryWriter.Write(screenshot);
                file.Close();
                Destroy(tex);
            }
            else
            {
                SSHDebug.LogError("Something went wrong! Texture is null");
            }
        }

        #if UNITY_EDITOR
        //causes crash
        //GameViewUtils.MaximizeGameView(false);
        GameViewUtils.SetSize(currentIndex);
        if (OnScreenChanged != null)
        {
            yield return new WaitForEndOfFrame();
            OnScreenChanged();
            yield return new WaitForEndOfFrame();
        }
        RemoveAllCustomSizes();
        
        #else
        Screen.SetResolution(initialRes.width, initialRes.height, false);
        #endif


        SSHDebug.Log("Screenshots saved to " + savePath);
        Application.OpenURL(savePath);
        isTakingShots = false;
        Time.timeScale = timeScaleStart;
    }

    private void RemoveAllCustomSizes()
    {
#if UNITY_EDITOR
        foreach (ShotSize shot in shotInfo)
        {
            GameViewUtils.RemoveCustomSize(shot.width, shot.height);
        }
#endif
    }

    public string GetScreenShotName(ShotSize shot)
    {

        string ext = shot.GetFileNameBase(); //shot.width.ToString() + "x" + shot.height.ToString();
        int num = GetFileNum(ext);

        string pre = "";
        if (!string.IsNullOrEmpty(shot.label))
            pre = shot.label + "_";
        return pre + ext + "_" + num.ToString() + ".png";
    }

    public delegate void DefaultsSetDelegate();
    public DefaultsSetDelegate DefaultsSet;
    public void SetDefaults()
    {

        keyToHold = KeyCode.LeftShift;
        keyToPress = KeyCode.S;
        orientation = SSHOrientation.portrait;
        shotInfo.Clear();
        shotInfo.Add(new ShotSize(640, 960)); // iPhone 4
        shotInfo.Add(new ShotSize(640, 1136)); // iPhone 5
        shotInfo.Add(new ShotSize(750, 1334)); // iPhone 6
        shotInfo.Add(new ShotSize(1080, 1920)); // Nexus 5
        shotInfo.Add(new ShotSize(1600, 2560)); // Galaxy Tab Pro
        shotInfo.Add(new ShotSize(1200, 1920));  // Nexus 7
        shotInfo.Add(new ShotSize(1242, 2208)); // iPhone 6 Plus
        shotInfo.Add(new ShotSize(1536, 2048)); // iPad 3 / 4 (OK for iPad mini)
        shotInfo.Add(new ShotSize(2048, 2732)); // iPad Pro
        savePath = "";

        buildSavePathExtra = "screenshots";
        buildSavePathRoot = Environment.SpecialFolder.MyPictures;

        if (DefaultsSet != null)
            DefaultsSet();
    }
}

public enum SSHOrientation
{
    portrait, landscape
}

public struct SSH_IntVector2
{
    public int width;
    public int height;

    public SSH_IntVector2(int w , int h)
    {
        this.width = w;
        this.height = h;
    }
}

[System.Serializable]
public class ShotSize
{
    [XmlAttribute("width")]
    public int width;
    [XmlAttribute("height")]
    public int height;
    [XmlAttribute("label")]
    public string label = "";

    public ShotSize()
    { }

    public ShotSize(int x, int y)
    {
        this.width = x;
        this.height = y;
    }

    public override string ToString()
    {
        return "(" + width.ToString() + "," + height.ToString() + ")";
    }

    public string GetFileNameBase()
    {
        return width.ToString() + "x" + height.ToString();
    }
}



public class SSHDebug
{
    private static string tag = "[Screenshot Helper] ";
    public static void Log(string message)
    {
        Debug.Log(tag + message);
    }

    public static void LogWarning(string message)
    {
        Debug.LogWarning(tag + message);
    }

    public static void LogError(string message)
    {
        Debug.LogError(tag + message);
    }
}


