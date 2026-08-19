using UnityEngine;

namespace VATSystem
{
    [CreateAssetMenu(fileName = "VATAnimationData", menuName = "VAT/Animation Data")]
    public class VATAnimationData : ScriptableObject
    {
        public string animationName;
        public Texture2D positionTexture;
        public Texture2D normalTexture;
        public Texture2D tangentTexture;
        public int vertexCount;
        public int totalFrames;
        public int textureWidth;
        public int rowsPerFrame;
        public float duration;
        public bool loop = true;
        public float speed = 1f;
    }
}