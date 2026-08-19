using System.IO;
using UnityEditor;
using UnityEngine;

namespace VATSystem
{
    public static class VATAssetUtility
    {
        public static Texture2D SaveTexture(Texture2D tex, string folder, string name)
        {
            string path = Path.Combine(folder, name + ".asset").Replace("\\", "/");
            AssetDatabase.CreateAsset(tex, path);
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp != null)
            {
                imp.sRGBTexture = false;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.mipmapEnabled = false;
                imp.filterMode = FilterMode.Point;
                imp.wrapMode = TextureWrapMode.Clamp;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public static Mesh SaveMesh(Mesh mesh, string folder, string name)
        {
            string path = Path.Combine(folder, name + ".asset").Replace("\\", "/");
            AssetDatabase.CreateAsset(mesh, path);
            return AssetDatabase.LoadAssetAtPath<Mesh>(path);
        }

        public static Material SaveMaterial(Material mat, string folder, string name)
        {
            string path = Path.Combine(folder, name + ".mat").Replace("\\", "/");
            AssetDatabase.CreateAsset(mat, path);
            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        public static void EnsureDir(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent))
                    EnsureDir(parent);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}