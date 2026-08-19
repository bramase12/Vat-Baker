using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VATSystem
{
    public static class VATAssetExporter
    {
        // ... (SaveTexture, SaveMesh, SaveMaterial, EnsureDir sama seperti di VATAssetUtility)

        public static VATAnimationDatabase CreateOrUpdateDatabase(string folder, List<GameObject> animationPrefabs)
        {
            return VATDatabaseBuilder.BuildOrUpdate(folder, animationPrefabs);
        }
    }
}