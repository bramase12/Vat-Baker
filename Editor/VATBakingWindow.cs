// ============================================================
// File: Assets/VAT/Scripts/Editor/VATBakingWindow.cs
// ============================================================
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VATSystem
{
    public class VATBakingWindow : EditorWindow
    {
        private GameObject sourceObject;
        private AnimationClip[] allClips;
        private bool[] clipSelection;
        private string outputRoot = "Assets/VAT";
        private float sampleRate = 30f;
        private bool generateTangents = true;
        private bool bakeRootMotion = false;

        [MenuItem("Tools/VAT/Bake Animations")]
        public static void ShowWindow() => GetWindow<VATBakingWindow>("VAT Baker");

        private void OnEnable() => RefreshClips();

        private void OnGUI()
        {
            EditorGUILayout.LabelField("VAT Baker (Playable Graph)", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            sourceObject = (GameObject)EditorGUILayout.ObjectField("Source Character", sourceObject, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck()) RefreshClips();

            if (sourceObject == null)
            {
                EditorGUILayout.HelpBox("Drag a GameObject with SkinnedMeshRenderer(s).", MessageType.Info);
                return;
            }

            var smrs = sourceObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            EditorGUILayout.LabelField($"Detected SMRs: {smrs.Length}");
            foreach (var s in smrs)
                EditorGUILayout.LabelField($"  • {s.name} ({s.sharedMesh?.vertexCount} verts, {s.sharedMaterials.Length} mats)");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Clips to Bake");
            if (allClips == null || allClips.Length == 0)
                EditorGUILayout.HelpBox("No AnimationClips found.", MessageType.Warning);
            else
                for (int i = 0; i < allClips.Length; i++)
                    clipSelection[i] = EditorGUILayout.ToggleLeft(allClips[i].name, clipSelection[i]);

            sampleRate = EditorGUILayout.FloatField("Sample Rate (FPS)", sampleRate);
            generateTangents = EditorGUILayout.Toggle("Generate Tangents", generateTangents);
            bakeRootMotion = EditorGUILayout.Toggle("Bake Root Motion", bakeRootMotion);
            outputRoot = EditorGUILayout.TextField("Output Root", outputRoot);

            GUI.enabled = allClips != null && clipSelection.Any(s => s);
            if (GUILayout.Button("Bake Selected Animations", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Confirm", "Start baking selected clips?", "Bake", "Cancel"))
                    BakeAnimations();
            }
            GUI.enabled = true;
        }

        private void RefreshClips()
        {
            if (sourceObject == null) { allClips = new AnimationClip[0]; clipSelection = new bool[0]; return; }
            var clips = new List<AnimationClip>();
            var animator = sourceObject.GetComponent<Animator>();
            if (animator && animator.runtimeAnimatorController)
                clips.AddRange(animator.runtimeAnimatorController.animationClips);
            var animation = sourceObject.GetComponent<Animation>();
            if (animation)
                foreach (AnimationState state in animation)
                    if (state.clip) clips.Add(state.clip);
            allClips = clips.Distinct().ToArray();
            clipSelection = new bool[allClips.Length];
        }

        private void BakeAnimations()
        {
            var selectedClips = allClips.Where((c, i) => clipSelection[i]).ToArray();
            if (selectedClips.Length == 0) return;

            if (!VATValidator.Validate(sourceObject))
            {
                EditorUtility.DisplayDialog("Error", "Validation failed. Check Console.", "OK");
                return;
            }

            // ---- Atur folder per karakter ----
            string characterFolder = Path.Combine(outputRoot, "Characters", sourceObject.name).Replace("\\", "/");
            string texRoot = Path.Combine(characterFolder, "Textures").Replace("\\", "/");
            string animPrefabsRoot = Path.Combine(characterFolder, "AnimationPrefabs").Replace("\\", "/");
            string databaseRoot = Path.Combine(characterFolder, "AnimationDatabase").Replace("\\", "/");

            VATAssetUtility.EnsureDir(texRoot);
            VATAssetUtility.EnsureDir(animPrefabsRoot);
            VATAssetUtility.EnsureDir(databaseRoot);
            VATAssetUtility.EnsureDir(characterFolder); // untuk menyimpan Character Prefab

            // ---- Persiapkan animator & root motion ----
            var animator = sourceObject.GetComponent<Animator>();
            if (animator == null) animator = sourceObject.AddComponent<Animator>();
            animator.enabled = false;

            Transform rootBone = sourceObject.transform;
            Vector3 rootPos0 = rootBone.localPosition;
            Quaternion rootRot0 = rootBone.localRotation;

            // ---- Kumpulkan semua SMR dan buat bakers ----
            var smrs = sourceObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            List<VATMeshBaker> allBakers = smrs.Select(smr => new VATMeshBaker(smr)).ToList();
            List<GameObject> animPrefabList = new List<GameObject>();

            // ---- Bake setiap clip ----
            foreach (var clip in selectedClips)
            {
                int frameCount = Mathf.RoundToInt(clip.length * sampleRate) + 1;
                float timeStep = clip.length / (frameCount - 1);

                var primaryBaker = allBakers[0]; // gunakan baker pertama sebagai contoh

                using (var sampler = new VATAnimationSampler(animator, clip))
                {
                    var positions = new List<Vector3>();
                    var normals = new List<Vector3>();
                    var tangents = new List<Vector4>();

                    for (int f = 0; f < frameCount; f++)
                    {
                        float t = f * timeStep;
                        sampler.Evaluate(t);

                        if (!bakeRootMotion)
                        {
                            rootBone.localPosition = rootPos0;
                            rootBone.localRotation = rootRot0;
                        }

                        primaryBaker.BakeFrame(positions, normals, tangents, generateTangents);
                    }

                    var layout = new VATTextureLayout(primaryBaker.VertexCount, frameCount);
                    Texture2D posTex = VATTextureEncoder.EncodePosition(positions.ToArray(), primaryBaker.VertexCount, frameCount, layout, TexturePrecision.RGBAHalf);
                    Texture2D nrmTex = VATTextureEncoder.EncodeNormal(normals.ToArray(), primaryBaker.VertexCount, frameCount, layout, TexturePrecision.RGBAHalf);
                    Texture2D tanTex = generateTangents ? VATTextureEncoder.EncodeTangent(tangents.ToArray(), primaryBaker.VertexCount, frameCount, layout, TexturePrecision.RGBAHalf) : null;

                    string safeName = clip.name.Replace(" ", "_");
                    var savedPos = VATAssetUtility.SaveTexture(posTex, texRoot, $"{safeName}_Pos");
                    var savedNrm = VATAssetUtility.SaveTexture(nrmTex, texRoot, $"{safeName}_Nrm");
                    Texture2D savedTan = tanTex ? VATAssetUtility.SaveTexture(tanTex, texRoot, $"{safeName}_Tan") : null;

                    var animData = ScriptableObject.CreateInstance<VATAnimationData>();
                    animData.animationName = clip.name;
                    animData.positionTexture = savedPos;
                    animData.normalTexture = savedNrm;
                    animData.tangentTexture = savedTan;
                    animData.vertexCount = primaryBaker.VertexCount;
                    animData.totalFrames = frameCount;
                    animData.textureWidth = layout.Width;
                    animData.rowsPerFrame = layout.RowsPerFrame;
                    animData.duration = clip.length;
                    animData.loop = clip.isLooping;
                    animData.speed = 1f;

                    // Simpan animData asset di folder animasi
                    string animFolder = Path.Combine(animPrefabsRoot, clip.name).Replace("\\", "/");
                    VATAssetUtility.EnsureDir(animFolder);
                    string animDataPath = Path.Combine(animFolder, $"{clip.name}.asset").Replace("\\", "/");
                    AssetDatabase.CreateAsset(animData, animDataPath);

                    // Bangun Animation Prefab (menggunakan semua baker agar semua bagian tubuh ikut)
                    VATAnimationPrefabBuilder.Build(sourceObject, allBakers, animData, animFolder);

                    // Kumpulkan prefab yang sudah dibuat
                    string prefabPath = Path.Combine(animFolder, $"{clip.name}.prefab").Replace("\\", "/");
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab != null) animPrefabList.Add(prefab);
                }
            }

            // ---- Bangun database animasi (per karakter) ----
            var database = VATDatabaseBuilder.BuildOrUpdate(databaseRoot, animPrefabList);

            // ---- Bangun Character Prefab (menggunakan default animasi Idle) ----
            VATCharacterPrefabBuilder.Build(sourceObject.name, database, allBakers, sourceObject, characterFolder);

            // ---- Bersihkan ----
            foreach (var baker in allBakers) baker.Dispose();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("VAT baking complete. Character saved in: " + characterFolder);
        }
    }
}