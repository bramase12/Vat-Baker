using System.IO;
using UnityEditor;
using UnityEngine;

namespace VATSystem
{
    public static class VATCharacterCreator
    {
        private const string ShaderName = "VAT/VAT";

        [MenuItem("Tools/VAT/Create Character Prefab")]
        public static void CreateCharacterPrefab()
        {
            var selected = Selection.activeObject as GameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Error", "Select a GameObject with SkinnedMeshRenderer.", "OK");
                return;
            }

            string dbPath = EditorUtility.OpenFilePanel("Select VATAnimationDatabase", "Assets/VAT/AnimationDatabase", "asset");
            if (string.IsNullOrEmpty(dbPath)) return;
            if (!dbPath.StartsWith(Application.dataPath)) return;
            string relativeDbPath = "Assets" + dbPath.Substring(Application.dataPath.Length);
            VATAnimationDatabase database = AssetDatabase.LoadAssetAtPath<VATAnimationDatabase>(relativeDbPath);
            if (database == null)
            {
                EditorUtility.DisplayDialog("Error", "Invalid VATAnimationDatabase.", "OK");
                return;
            }

            // Dapatkan animasi default (Idle atau yang pertama)
            VATAnimationData defaultAnim = null;
            GameObject defaultPrefab = database.GetAnimationPrefab("Idle");
            if (defaultPrefab == null && database.animations.Count > 0)
                defaultPrefab = database.animations[0].animationPrefab;
            if (defaultPrefab != null)
            {
                var prefabAnimator = defaultPrefab.GetComponent<VATAnimator>();
                if (prefabAnimator != null) defaultAnim = prefabAnimator.singleClipData;
            }

            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                EditorUtility.DisplayDialog("Error", $"Shader '{ShaderName}' not found.", "OK");
                return;
            }

            string charsFolder = "Assets/VAT/Character";
            string meshFolder = "Assets/VAT/Meshes";
            string matFolder = "Assets/VAT/Materials";
            VATAssetUtility.EnsureDir(charsFolder);
            VATAssetUtility.EnsureDir(meshFolder);
            VATAssetUtility.EnsureDir(matFolder);

            GameObject instance = Object.Instantiate(selected);
            instance.name = selected.name;

            var smrs = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var smr in smrs)
            {
                Mesh origMesh = smr.sharedMesh;
                Material[] origMats = smr.sharedMaterials;
                GameObject go = smr.gameObject;
                Object.DestroyImmediate(smr);

                // Buat VAT mesh
                Mesh vatMesh = CreateVATMesh(origMesh);
                string meshName = origMesh.name + "_VAT";
                Mesh savedMesh = VATAssetUtility.SaveMesh(vatMesh, meshFolder, meshName);

                Material[] vatMaterials = new Material[origMats.Length];
                for (int i = 0; i < origMats.Length; i++)
                {
                    Material orig = origMats[i];
                    Material vatMat = new Material(shader);
                    vatMat.name = (orig ? orig.name : "Material") + "_VAT";
                    if (orig != null)
                    {
                        vatMat.CopyPropertiesFromMaterial(orig);
                        foreach (var kw in orig.shaderKeywords) vatMat.EnableKeyword(kw);
                    }

                    if (defaultAnim != null)
                    {
                        vatMat.SetTexture("_PosTex", defaultAnim.positionTexture);
                        vatMat.SetTexture("_NrmTex", defaultAnim.normalTexture);
                        if (defaultAnim.tangentTexture != null) vatMat.SetTexture("_TanTex", defaultAnim.tangentTexture);
                        vatMat.SetFloat("_VAT_VertexCount", defaultAnim.vertexCount);
                        vatMat.SetFloat("_VAT_TotalFrames", defaultAnim.totalFrames);
                        vatMat.SetFloat("_VAT_TexWidth", defaultAnim.textureWidth);
                        vatMat.SetFloat("_VAT_RowsPerFrame", defaultAnim.rowsPerFrame);
                    }

                    vatMaterials[i] = VATAssetUtility.SaveMaterial(vatMat, matFolder, vatMat.name);
                }

                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = savedMesh;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterials = vatMaterials;
            }

            // Tambah VATAnimator (nama variabel berbeda dengan di atas)
            var animatorComponent = instance.AddComponent<VATAnimator>();
            animatorComponent.animationDatabase = database;
            animatorComponent.currentAnimation = defaultAnim != null ? defaultAnim.animationName : "";
            animatorComponent.playOnStart = true;

            // Di bagian akhir, simpan prefab dengan nama asli
            // ... bagian akhir:
            string prefabPath = Path.Combine(charsFolder, instance.name + ".prefab").Replace("\\", "/");
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Character prefab created at {prefabPath}");
        }

        private static Mesh CreateVATMesh(Mesh source)
        {
            Mesh mesh = Object.Instantiate(source);
            mesh.name = source.name + "_VAT";
            int vc = mesh.vertexCount;
            Vector2[] uv1 = new Vector2[vc];
            float inv = 1f / vc;
            for (int i = 0; i < vc; i++)
                uv1[i] = new Vector2((i + 0.5f) * inv, 0f);
            mesh.SetUVs(1, uv1);
            mesh.UploadMeshData(false);
            return mesh;
        }

        
    }
}