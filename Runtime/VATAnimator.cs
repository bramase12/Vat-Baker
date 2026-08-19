using UnityEngine;

namespace VATSystem
{
    public class VATAnimator : MonoBehaviour
    {
        [Header("Animation Source")]
        public VATAnimationDatabase animationDatabase;
        public VATAnimationData singleClipData;   // untuk prefab mandiri

        [Header("Playback")]
        public string currentAnimation = "";
        public float speed = 1f;
        public bool loop = true;
        public bool playOnStart = true;

        private MeshFilter[] meshFilters;
        private MeshRenderer[] meshRenderers;
        private VATAnimationData currentClipData;
        private float currentFrame;
        private bool isPlaying;

        public string CurrentAnimation => currentAnimation;
        public float CurrentFrame => currentFrame;
        public bool IsPlaying => isPlaying;

        private static readonly int _PosTex = Shader.PropertyToID("_PosTex");
        private static readonly int _NrmTex = Shader.PropertyToID("_NrmTex");
        private static readonly int _TanTex = Shader.PropertyToID("_TanTex");
        private static readonly int _VertexCount = Shader.PropertyToID("_VAT_VertexCount");
        private static readonly int _TotalFrames = Shader.PropertyToID("_VAT_TotalFrames");
        private static readonly int _TexWidth = Shader.PropertyToID("_VAT_TexWidth");
        private static readonly int _RowsPerFrame = Shader.PropertyToID("_VAT_RowsPerFrame");
        private static readonly int _AnimFrameA = Shader.PropertyToID("_AnimFrameA");
        private static readonly int _AnimFrameB = Shader.PropertyToID("_AnimFrameB");
        private static readonly int _BlendWeight = Shader.PropertyToID("_BlendWeight");

        private void Awake()
        {
            meshFilters = GetComponentsInChildren<MeshFilter>(true);
            meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        }

        private void Start()
        {
            if (singleClipData != null)
                PlaySingleClip(singleClipData);
            else if (playOnStart && animationDatabase != null && !string.IsNullOrEmpty(currentAnimation))
                Play(currentAnimation);
        }

        private void Update()
        {
            if (!isPlaying || currentClipData == null) return;

            float frameRate = currentClipData.totalFrames / currentClipData.duration;
            currentFrame += speed * currentClipData.speed * frameRate * Time.deltaTime;

            if (loop && currentClipData.loop)
            {
                if (currentFrame >= currentClipData.totalFrames)
                    currentFrame = currentFrame % currentClipData.totalFrames;
            }
            else if (currentFrame >= currentClipData.totalFrames)
            {
                currentFrame = currentClipData.totalFrames - 1;
                isPlaying = false;
                return;
            }

            if (meshRenderers == null || meshRenderers.Length == 0) return;

            var block = new MaterialPropertyBlock();
            foreach (var mr in meshRenderers)
            {
                if (mr == null) continue;
                mr.GetPropertyBlock(block);

                block.SetTexture(_PosTex, currentClipData.positionTexture);
                block.SetTexture(_NrmTex, currentClipData.normalTexture);
                if (currentClipData.tangentTexture != null)
                    block.SetTexture(_TanTex, currentClipData.tangentTexture);
                block.SetFloat(_VertexCount, currentClipData.vertexCount);
                block.SetFloat(_TotalFrames, currentClipData.totalFrames);
                block.SetFloat(_TexWidth, currentClipData.textureWidth);
                block.SetFloat(_RowsPerFrame, currentClipData.rowsPerFrame);
                block.SetFloat(_AnimFrameA, currentFrame);
                block.SetFloat(_AnimFrameB, currentFrame);
                block.SetFloat(_BlendWeight, 1f);

                mr.SetPropertyBlock(block);
            }
        }

        public void Play(string animationName, float normalizedTime = 0f)
        {
            if (animationDatabase == null)
            {
                Debug.LogError("VATAnimator: Animation Database not assigned.");
                return;
            }

            GameObject prefab = animationDatabase.GetAnimationPrefab(animationName);
            if (prefab == null)
            {
                Debug.LogError($"VATAnimator: Animation '{animationName}' not found in database.");
                return;
            }

            ApplyAnimationPrefab(prefab);
            currentAnimation = animationName;
            currentFrame = Mathf.Clamp(normalizedTime * currentClipData.totalFrames, 0, currentClipData.totalFrames - 1);
            isPlaying = true;
        }

        public void PlaySingleClip(VATAnimationData clipData, float normalizedTime = 0f)
        {
            if (clipData == null)
            {
                Debug.LogError("VATAnimator: Single clip data is null.");
                return;
            }
            currentClipData = clipData;
            currentAnimation = clipData.animationName;
            currentFrame = Mathf.Clamp(normalizedTime * clipData.totalFrames, 0, clipData.totalFrames - 1);
            isPlaying = true;
        }

        private void ApplyAnimationPrefab(GameObject animationPrefab)
        {
            if (animationPrefab == null) return;

            var prefabVATAnim = animationPrefab.GetComponent<VATAnimator>();
            if (prefabVATAnim != null && prefabVATAnim.singleClipData != null)
                currentClipData = prefabVATAnim.singleClipData;

            var prefabMFs = animationPrefab.GetComponentsInChildren<MeshFilter>(true);
            var prefabMRs = animationPrefab.GetComponentsInChildren<MeshRenderer>(true);

            if (meshFilters != null && prefabMFs != null)
            {
                for (int i = 0; i < meshFilters.Length && i < prefabMFs.Length; i++)
                {
                    if (meshFilters[i] != null && prefabMFs[i] != null)
                        meshFilters[i].sharedMesh = prefabMFs[i].sharedMesh;
                }
            }

            if (meshRenderers != null && prefabMRs != null)
            {
                for (int i = 0; i < meshRenderers.Length && i < prefabMRs.Length; i++)
                {
                    if (meshRenderers[i] != null && prefabMRs[i] != null)
                        meshRenderers[i].sharedMaterials = prefabMRs[i].sharedMaterials;
                }
            }
        }

        public void Stop() => isPlaying = false;
        public void Pause() => isPlaying = false;
        public void Resume() => isPlaying = true;
    }
}