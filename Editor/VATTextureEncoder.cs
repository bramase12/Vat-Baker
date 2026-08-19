using UnityEngine;

namespace VATSystem
{
    public static class VATTextureEncoder
    {
        public static Texture2D EncodePosition(Vector3[] data, int vertexCount, int totalFrames, VATTextureLayout layout, TexturePrecision precision)
            => Encode(data, vertexCount, totalFrames, layout, precision, false);

        public static Texture2D EncodeNormal(Vector3[] data, int vertexCount, int totalFrames, VATTextureLayout layout, TexturePrecision precision)
            => Encode(data, vertexCount, totalFrames, layout, precision, true);

        public static Texture2D EncodeTangent(Vector4[] data, int vertexCount, int totalFrames, VATTextureLayout layout, TexturePrecision precision)
        {
            var tex = new Texture2D(layout.Width, layout.Height, GetFormat(precision), false, true);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[layout.Width * layout.Height];
            for (int f = 0; f < totalFrames; f++)
                for (int v = 0; v < vertexCount; v++)
                {
                    Vector4 val = data[f * vertexCount + v];
                    var (px, py) = layout.GetPixel(v, f);
                    pixels[py * layout.Width + px] = new Color(val.x, val.y, val.z, val.w);
                }
            tex.SetPixels(pixels);
            tex.Apply(false, true);
            return tex;
        }

        private static Texture2D Encode(Vector3[] data, int vertexCount, int totalFrames, VATTextureLayout layout, TexturePrecision precision, bool isNormal)
        {
            var tex = new Texture2D(layout.Width, layout.Height, GetFormat(precision), false, true);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[layout.Width * layout.Height];
            for (int f = 0; f < totalFrames; f++)
                for (int v = 0; v < vertexCount; v++)
                {
                    Vector3 val = data[f * vertexCount + v];
                    if (isNormal) val = val * 0.5f + Vector3.one * 0.5f;
                    var (px, py) = layout.GetPixel(v, f);
                    pixels[py * layout.Width + px] = new Color(val.x, val.y, val.z, 0f);
                }
            tex.SetPixels(pixels);
            tex.Apply(false, true);
            return tex;
        }

        private static TextureFormat GetFormat(TexturePrecision precision)
        {
            return precision == TexturePrecision.RGBAFloat ? TextureFormat.RGBAFloat : TextureFormat.RGBAHalf;
        }
    }
}