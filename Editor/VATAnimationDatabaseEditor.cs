using System.IO;
using UnityEditor;
using UnityEngine;

namespace VATSystem
{
    [CustomEditor(typeof(VATAnimationDatabase))]
    public class VATAnimationDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var db = (VATAnimationDatabase)target;
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("animations"), true);

            if (GUILayout.Button("Rebuild from Prefab Folder"))
            {
                RebuildDatabaseFromFolder(db);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void RebuildDatabaseFromFolder(VATAnimationDatabase db)
        {
            string folder = "Assets/VAT/AnimationPrefabs";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogError("AnimationPrefabs folder not found.");
                return;
            }
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            db.animations.Clear();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    db.AddOrUpdateAnimation(prefab.name, prefab);
                }
            }
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
        }
    }
}