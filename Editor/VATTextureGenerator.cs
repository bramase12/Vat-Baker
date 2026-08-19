using UnityEditor;
using UnityEngine;

namespace VATSystem
{
    public enum TexturePrecision { RGBAHalf, RGBAFloat }

    public static class VATTextureGenerator
    {
        private const int MaxWidth = 16384;

        public static Texture2D CreatePositionTexture(Vector3[] data, int vertexCount, int totalFrames, TexturePrecision precision = TexturePrecision.RGBAHalf)
            => CreateTexture(data, vertexCount, totalFrames, precision, false);

        public static Texture2D CreateNormalTexture(Vector3[] data, int vertexCount, int totalFrames, TexturePrecision precision = TexturePrecision.RGBAHalf)
            => CreateTexture(data, vertexCount, totalFrames, precision, true);

        public static Texture2D CreateTangentTexture(Vector4[] data, int vertexCount, int totalFrames, TexturePrecision precision = TexturePrecision.RGBAHalf)
        {
            (int w, int h, int rpf) = GetDims(vertexCount, totalFrames);
            var fmt = precision == TexturePrecision.RGBAFloat ? TextureFormat.RGBAFloat : TextureFormat.RGBAHalf;
            var tex = new Texture2D(w, h, fmt, false, true);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[w * h];
            for (int f = 0; f < totalFrames; f++)
                for (int v = 0; v < vertexCount; v++)
                {
                    Vector4 val = data[f * vertexCount + v];
                    (int px, int py) = Pixel(v, f, w, rpf);
                    pixels[py * w + px] = new Color(val.x, val.y, val.z, val.w);
                }
            tex.SetPixels(pixels);
            tex.Apply(false, true);
            return tex;
        }

        private static Texture2D CreateTexture(Vector3[] data, int vertexCount, int totalFrames, TexturePrecision precision, bool isNormal)
        {
            (int w, int h, int rpf) = GetDims(vertexCount, totalFrames);
            var fmt = precision == TexturePrecision.RGBAFloat ? TextureFormat.RGBAFloat : TextureFormat.RGBAHalf;
            var tex = new Texture2D(w, h, fmt, false, true);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[w * h];
            for (int f = 0; f < totalFrames; f++)
                for (int v = 0; v < vertexCount; v++)
                {
                    Vector3 val = data[f * vertexCount + v];
                    if (isNormal) val = val * 0.5f + Vector3.one * 0.5f;
                    (int px, int py) = Pixel(v, f, w, rpf);
                    pixels[py * w + px] = new Color(val.x, val.y, val.z, 0f);
                }
            tex.SetPixels(pixels);
            tex.Apply(false, true);
            return tex;
        }

        public static (int width, int height, int rowsPerFrame) GetDims(int vertexCount, int totalFrames)
        {
            int rpf = Mathf.CeilToInt((float)vertexCount / MaxWidth);
            int w = Mathf.Min(vertexCount, MaxWidth);
            int h = rpf * totalFrames;
            return (w, h, rpf);
        }

        private static (int x, int y) Pixel(int v, int f, int w, int rpf)
        {
            int row = v / w;
            int col = v % w;
            return (col, f * rpf + row);
        }

        public static void SaveTexture(Texture2D tex, string path)
        {
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
        }

        // Overload tanpa precision (default RGBAHalf)
        public static Texture2D CreatePositionTexture(Vector3[] data, int vertexCount, int totalFrames)
            => CreatePositionTexture(data, vertexCount, totalFrames, TexturePrecision.RGBAHalf);
        public static Texture2D CreateNormalTexture(Vector3[] data, int vertexCount, int totalFrames)
            => CreateNormalTexture(data, vertexCount, totalFrames, TexturePrecision.RGBAHalf);
        public static Texture2D CreateTangentTexture(Vector4[] data, int vertexCount, int totalFrames)
            => CreateTangentTexture(data, vertexCount, totalFrames, TexturePrecision.RGBAHalf);
        public static (int, int, int) GetTextureDimensions(int vertexCount, int totalFrames)
            => GetDims(vertexCount, totalFrames);

        [MenuItem("Tools/VAT/Fix All Texture Import Settings")]
       // Pastikan method ini ada di VATTextureGenerator.cs (jika belum)
        public static void FixAllTextureImports()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/VAT/Textures" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp != null)
                {
                    bool dirty = false;
                    if (imp.sRGBTexture) { imp.sRGBTexture = false; dirty = true; }
                    if (imp.textureCompression != TextureImporterCompression.Uncompressed) { imp.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
                    if (imp.mipmapEnabled) { imp.mipmapEnabled = false; dirty = true; }
                    if (imp.filterMode != FilterMode.Point) { imp.filterMode = FilterMode.Point; dirty = true; }
                    if (imp.wrapMode != TextureWrapMode.Clamp) { imp.wrapMode = TextureWrapMode.Clamp; dirty = true; }
                    if (dirty) imp.SaveAndReimport();
                }
            }
            Debug.Log("All VAT textures fixed.");
        }
        
    }
}