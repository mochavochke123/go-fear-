using UnityEngine;
using UnityEditor;

public class CleanMissingScripts : Editor
{
    [MenuItem("Tools/Clean Missing Scripts From Selected")]
    public static void CleanSelected()
    {
        int count = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            SerializedObject so = new SerializedObject(go);
            SerializedProperty sp = so.FindProperty("m_Component");
            
            if (sp != null && sp.isArray)
            {
                int deleteCount = 0;
                for (int i = sp.arraySize - 1; i >= 0; i--)
                {
                    SerializedProperty comp = sp.GetArrayElementAtIndex(i);
                    if (comp.FindPropertyRelative("component").objectReferenceValue == null)
                    {
                        sp.DeleteArrayElementAtIndex(i);
                        deleteCount++;
                    }
                }
                if (deleteCount > 0)
                {
                    so.ApplyModifiedProperties();
                    count += deleteCount;
                    Debug.Log($"Cleaned {deleteCount} from {go.name}");
                }
            }
        }
        Debug.Log($"Total: cleaned {count} missing scripts");
    }
}