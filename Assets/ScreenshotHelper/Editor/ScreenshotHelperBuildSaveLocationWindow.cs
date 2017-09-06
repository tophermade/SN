using UnityEngine;
using UnityEditor;

public class ScreenshotHelperBuildSaveLocationWindow : EditorWindow
{
    public static ScreenshotHelper _ssh;
    public static void ShowWindow(ScreenshotHelper ssh)
    {
        EditorWindow.GetWindow(typeof(ScreenshotHelperBuildSaveLocationWindow));
        _ssh = ssh;
    }

    
    void OnGUI()
    {
        if (_ssh == null)
        {
            EditorWindow sshWindow = EditorWindow.GetWindow(typeof(ScreenshotHelperBuildSaveLocationWindow));
            if (sshWindow != null)
                sshWindow.Close();
            return;
        }

        EditorGUILayout.HelpBox("Set the location to save screenshots when using a build. The exact location will change based on the machine the build is running. For example the user name will be corrected.", MessageType.None);


        string example = _ssh.BuildSaveLocation();
        EditorGUILayout.HelpBox("Location example: " + example, MessageType.None);

        _ssh.buildSavePathRoot = (System.Environment.SpecialFolder)EditorGUILayout.EnumPopup("Root: ", _ssh.buildSavePathRoot);
        _ssh.buildSavePathExtra = EditorGUILayout.TextField("Extra directory: ", _ssh.buildSavePathExtra);
    }
}