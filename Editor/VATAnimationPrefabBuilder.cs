using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VATSystem
{
    public static class VATAnimationPrefabBuilder
    {
        /// <summary>
        /// Membuat sebuah Animation Prefab yang berisi mesh VAT dan material VAT
        /// untuk setiap bagian tubuh karakter. Semua aset disimpan sebagai aset permanen.
        /// </summary>
        public static void Build(GameObject sourceCharacter, List<VATMeshBaker> bakers,
            VATAnimationData animData, string outputFolder)
        {
            // 1. Instantiate karakter sumber (mempertahankan seluruh hierarki)
            GameObject instance = Object.Instantiate(sourceCharacter);
            instance.name = animData.animationName;

            // Hapus Animator jika ada
            Animator anim = instance.GetComponent<Animator>();
            if (anim != null) Object.DestroyImmediate(anim);

            // 2. Ganti setiap SkinnedMeshRenderer dengan MeshRenderer + MeshFilter
            var smrs = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            bool anySuccess = false;
            int vertexOffset = 0;

            for (int i = 0; i < smrs.Length && i < bakers.Count; i++)
            {
                var smr = smrs[i];
                var baker = bakers[i];
                GameObject go = smr.gameObject;

                // Hapus SMR
                Object.DestroyImmediate(smr);

                // --- Buat mesh VAT (salinan mesh asli + UV1) dan simpan sebagai aset permanen ---
                Mesh vatMesh = baker.CreateStaticVATMesh(vertexOffset, animData.vertexCount);
                vertexOffset += baker.VertexCount;
                if (vatMesh == null || vatMesh.vertexCount == 0)
                {
                    Debug.LogError($"Gagal membuat VAT mesh untuk {go.name}");
                    continue;
                }

                string meshDir = Path.Combine(outputFolder, "Meshes").Replace("\\", "/");
                VATAssetUtility.EnsureDir(meshDir);
                Mesh savedMesh = VATAssetUtility.SaveMesh(vatMesh, meshDir, $"{animData.animationName}_{go.name}_Mesh");
                if (savedMesh == null)
                {
                    Debug.LogError($"Gagal menyimpan mesh ke disk untuk {go.name}");
                    continue;
                }

                // --- Buat material VAT untuk setiap slot material asli dan simpan sebagai aset permanen ---
                Material[] origMats = baker.SkinnedMeshRenderer.sharedMaterials;
                Material[] vatMats = new Material[origMats.Length];
                string matDir = Path.Combine(outputFolder, "Materials").Replace("\\", "/");
                VATAssetUtility.EnsureDir(matDir);

                for (int m = 0; m < origMats.Length; m++)
                {
                    Material vatMat = VATMaterialGenerator.CreateVATMaterial(origMats[m], animData);
                    if (vatMat != null)
                        vatMats[m] = VATAssetUtility.SaveMaterial(vatMat, matDir, vatMat.name);
                    else
                    {
                        // Fallback: gunakan material asli agar tidak kosong
                        Debug.LogWarning($"Material VAT gagal dibuat untuk {origMats[m]?.name}. Memakai material asli.");
                        vatMats[m] = origMats[m];
                    }
                }

                // Pasang komponen
                MeshFilter mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = savedMesh;
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterials = vatMats;

                anySuccess = true;
            }

            // Jika tidak ada satu pun mesh berhasil, batalkan
            if (!anySuccess)
            {
                Debug.LogError("Tidak ada mesh yang berhasil dibuat. Prefab animasi tidak akan disimpan.");
                Object.DestroyImmediate(instance);
                return;
            }

            // 3. Tambahkan VATAnimator dengan singleClipData (untuk prefab mandiri)
            var vatAnim = instance.AddComponent<VATAnimator>();
            vatAnim.singleClipData = animData;
            vatAnim.playOnStart = true;

            // 4. Simpan prefab
            string prefabPath = Path.Combine(outputFolder, $"{animData.animationName}.prefab").Replace("\\", "/");
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            // Verifikasi akhir
            var savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (savedPrefab != null)
            {
                var mfs = savedPrefab.GetComponentsInChildren<MeshFilter>(true);
                var mrs = savedPrefab.GetComponentsInChildren<MeshRenderer>(true);
                if (mfs.Length == 0 || mrs.Length == 0)
                    Debug.LogError($"Animation Prefab '{animData.animationName}' TIDAK memiliki mesh atau material!");
                else
                    Debug.Log($"Animation Prefab '{animData.animationName}' dibuat dengan {mfs.Length} mesh(es) dan {mrs.Length} renderer(s).");
            }
        }
    }
}