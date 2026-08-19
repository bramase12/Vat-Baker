using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VATSystem
{
    public static class VATAnimationDatabaseBuilder
    {
        public static VATAnimationDatabase BuildOrUpdate(string databaseFolder, List<GameObject> animationPrefabs)
        {
            VATAssetUtility.EnsureDir(databaseFolder);
            string dbPath = Path.Combine(databaseFolder, "VATAnimationDatabase.asset").Replace("\\", "/");

            VATAnimationDatabase db = AssetDatabase.LoadAssetAtPath<VATAnimationDatabase>(dbPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<VATAnimationDatabase>();
                AssetDatabase.CreateAsset(db, dbPath);
            }

            foreach (var prefab in animationPrefabs)
            {
                if (prefab == null) continue;
                string animName = prefab.name;
                db.AddOrUpdateAnimation(animName, prefab);   // <-- perbaikan
            }

            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            return db;
        }
    }
}