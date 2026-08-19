using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VATSystem
{
    public static class VATCharacterPrefabBuilder
    {
        public static void Build(string characterName, VATAnimationDatabase database,
            List<VATMeshBaker> bakers, GameObject sourceCharacter, string characterFolder)
        {
            // 1. Coba dapatkan Animation Prefab default (Idle)
            GameObject defaultPrefab = database?.GetAnimationPrefab("Idle");
            if (defaultPrefab == null && database != null && database.animations.Count > 0)
                defaultPrefab = database.animations[0].animationPrefab;

            GameObject character = null;

            // 2. Jika ada defaultPrefab, gunakan sebagai basis
            if (defaultPrefab != null)
            {
                character = Object.Instantiate(defaultPrefab);
                character.name = characterName;

                // Hapus VATAnimator bawaan
                var oldAnim = character.GetComponent<VATAnimator>();
                if (oldAnim != null) Object.DestroyImmediate(oldAnim);

                // Verifikasi bahwa prefab memiliki mesh dan material
                var mfs = character.GetComponentsInChildren<MeshFilter>(true);
                var mrs = character.GetComponentsInChildren<MeshRenderer>(true);
                if (mfs.Length == 0 || mrs.Length == 0)
                {
                    Debug.LogWarning("Animation Prefab default tidak memiliki mesh/material. Fallback ke sumber asli.");
                    Object.DestroyImmediate(character);
                    character = null;
                }
            }

            // 3. Fallback: bangun dari sumber asli menggunakan bakers
            if (character == null)
            {
                Debug.Log("Membangun karakter dari sumber asli dengan bakers...");
                character = Object.Instantiate(sourceCharacter);
                character.name = characterName;

                // Hapus Animator
                var anim = character.GetComponent<Animator>();
                if (anim != null) Object.DestroyImmediate(anim);

                // Ganti semua SkinnedMeshRenderer dengan MeshRenderer + MeshFilter
                var smrs = character.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                bool anySuccess = false;

                for (int i = 0; i < smrs.Length && i < bakers.Count; i++)
                {
                    var smr = smrs[i];
                    var baker = bakers[i];
                    GameObject go = smr.gameObject;
                    Object.DestroyImmediate(smr);

                    // Buat mesh VAT dan simpan sebagai aset permanen
                    Mesh vatMesh = baker.CreateStaticVATMesh();
                    if (vatMesh == null || vatMesh.vertexCount == 0) continue;

                    string meshDir = Path.Combine(characterFolder, "Meshes").Replace("\\", "/");
                    VATAssetUtility.EnsureDir(meshDir);
                    Mesh savedMesh = VATAssetUtility.SaveMesh(vatMesh, meshDir, $"{characterName}_{go.name}_Mesh");
                    if (savedMesh == null) continue;

                    // Buat material VAT (placeholder dengan texture hitam, nanti diganti saat animasi berjalan)
                    Material[] origMats = baker.SkinnedMeshRenderer.sharedMaterials;
                    Material[] vatMats = new Material[origMats.Length];
                    string matDir = Path.Combine(characterFolder, "Materials").Replace("\\", "/");
                    VATAssetUtility.EnsureDir(matDir);

                    for (int m = 0; m < origMats.Length; m++)
                    {
                        Material orig = origMats[m];
                        Material vatMat = new Material(Shader.Find("VAT/VAT"));
                        vatMat.name = (orig != null ? orig.name : "Material") + "_Char";
                        if (orig != null)
                        {
                            vatMat.CopyPropertiesFromMaterial(orig);
                            foreach (var kw in orig.shaderKeywords) vatMat.EnableKeyword(kw);
                            // Paksa opaque
                            Color c = vatMat.GetColor("_Color");
                            c.a = 1f;
                            vatMat.SetColor("_Color", c);
                            vatMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                            vatMat.DisableKeyword("_ALPHABLEND_ON");
                            vatMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        }
                        // Isi texture VAT placeholder (hitam)
                        vatMat.SetTexture("_PosTex", Texture2D.blackTexture);
                        vatMat.SetTexture("_NrmTex", Texture2D.blackTexture);
                        vatMat.SetFloat("_VAT_VertexCount", vatMesh.vertexCount);
                        vatMat.SetFloat("_VAT_TotalFrames", 1);
                        vatMat.SetFloat("_VAT_TexWidth", 256);
                        vatMat.SetFloat("_VAT_RowsPerFrame", 1);

                        vatMats[m] = VATAssetUtility.SaveMaterial(vatMat, matDir, vatMat.name);
                    }

                    MeshFilter mf = go.AddComponent<MeshFilter>();
                    mf.sharedMesh = savedMesh;
                    MeshRenderer mr = go.AddComponent<MeshRenderer>();
                    mr.sharedMaterials = vatMats;

                    anySuccess = true;
                }

                if (!anySuccess)
                    Debug.LogError("Tidak ada mesh yang berhasil dibuat untuk karakter.");
            }

            // 4. Tambahkan VATAnimator dengan database
            var vatAnim = character.AddComponent<VATAnimator>();
            vatAnim.animationDatabase = database;
            vatAnim.currentAnimation = "Idle";
            vatAnim.playOnStart = true;

            // 5. Simpan prefab karakter
            string prefabPath = Path.Combine(characterFolder, $"{characterName}.prefab").Replace("\\", "/");
            PrefabUtility.SaveAsPrefabAsset(character, prefabPath);
            Object.DestroyImmediate(character);

            // Verifikasi akhir
            var savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (savedPrefab != null)
            {
                var mfs = savedPrefab.GetComponentsInChildren<MeshFilter>(true);
                var mrs = savedPrefab.GetComponentsInChildren<MeshRenderer>(true);
                if (mfs.Length == 0 || mrs.Length == 0)
                    Debug.LogError($"Character Prefab '{characterName}' TIDAK memiliki mesh atau material!");
                else
                    Debug.Log($"Character Prefab '{characterName}' berhasil dibuat dengan {mfs.Length} mesh(es) dan {mrs.Length} renderer(s).");
            }
        }
    }
}