using UnityEngine;

namespace VATSystem
{
    public class VATTextureLayout
    {
        private const int MaxWidth = 16384;

        public int Width { get; }
        public int Height { get; }
        public int RowsPerFrame { get; }

        public VATTextureLayout(int vertexCount, int totalFrames)
        {
            RowsPerFrame = Mathf.CeilToInt((float)vertexCount / MaxWidth);
            Width = Mathf.Min(vertexCount, MaxWidth);
            Height = RowsPerFrame * totalFrames;
        }

        public (int x, int y) GetPixel(int vertexIndex, int frame)
        {
            int row = vertexIndex / Width;
            int col = vertexIndex % Width;
            return (col, frame * RowsPerFrame + row);
        }
    }
}