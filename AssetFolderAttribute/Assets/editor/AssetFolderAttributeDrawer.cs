using UnityEngine;
using UnityEditor;
using System.IO;

[CustomPropertyDrawer(typeof(AssetFolderAttribute))]
/**
* AssetFolderAttributeDrawer - A class to draw inspector ui for asset folder properties
**/
public class AssetFolderAttributeDrawer : PropertyDrawer
{
    // Necessary since some properties tend to collapse smaller than their content
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    private static string getFullPath(string folderName, string assetRootPath)
    {
        string fullPath;
        if (Path.IsPathRooted(folderName))
        {
            fullPath = folderName;
        }
        else
        {
            fullPath = Path.Combine(assetRootPath, folderName);
        }
        return fullPath;
    }

    private static string getAssetRelativePath(string folderName, string assetRootPath)
    {
        //Verify that the folder is under the root asset path for the Unity project
        if (folderName.ToLower().StartsWith(assetRootPath))
        {
            folderName = folderName.Substring(assetRootPath.Length);

            //remove leading path seperator
            if (folderName.StartsWith(Path.DirectorySeparatorChar.ToString()) ||
                folderName.StartsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                folderName = folderName.Remove(0, 1);
            }
        }

        return folderName;
    }

    // Draw a disabled property field
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var folderName = property.stringValue;
        
        GUILayout.BeginHorizontal(GUIStyle.none);

        var textDimensions = GUI.skin.label.CalcSize(label);
        var assetRootPathLower = Application.dataPath.ToLower();

        var oldColor = GUI.backgroundColor;
        string fullPath = getFullPath(folderName, assetRootPathLower);

        //draw the field in red if not a valid asset path   
        string labelText = label.text;
        if (!fullPath.ToLower().StartsWith(assetRootPathLower))
        {
            GUI.backgroundColor = Color.red;
            labelText += "(not an asset path)";
        }
        else if (!Directory.Exists(fullPath))
        {
            GUI.backgroundColor = Color.red;
            labelText += "(path not found)";
        }

        label.text = labelText;
        folderName = getAssetRelativePath(EditorGUILayout.TextField(label, folderName), assetRootPathLower);

        GUI.backgroundColor = oldColor;

        GUI.SetNextControlName("FolderButton");

        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            //folder name might have changed by now
            fullPath = getFullPath(folderName, assetRootPathLower);

            var newFullPath = EditorUtility.OpenFolderPanel("Select " + label.text, fullPath, "");
            if (newFullPath.Length > 0)//if user cancels returns 0-length string
            {
                folderName = getAssetRelativePath(newFullPath, assetRootPathLower);
            }
            GUI.FocusControl("FolderButton");//forces focus to the button to force text field to update (this feels like a bug in the Unity gui)
        }

        if (folderName != property.stringValue)
        {
            //If the value changed we need to update the UI
            // not sure which of these are actually required
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
            EditorApplication.update.Invoke();
        }

        property.stringValue = folderName;

        GUILayout.EndHorizontal();
    }
}
