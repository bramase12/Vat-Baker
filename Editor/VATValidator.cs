using UnityEditor;
using UnityEngine;

namespace VATSystem
{
    public static class VATValidator
    {
        public static bool Validate(GameObject source)
        {
            if (source == null)
            {
                Debug.LogError("VATValidator: Source object is null.");
                return false;
            }
            var animator = source.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("VATValidator: No Animator found.");
                return false;
            }
            var smrs = source.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (smrs.Length == 0)
            {
                Debug.LogError("VATValidator: No SkinnedMeshRenderer found.");
                return false;
            }
            foreach (var smr in smrs)
            {
                if (smr.sharedMesh == null)
                {
                    Debug.LogError($"VATValidator: {smr.name} has no mesh.");
                    return false;
                }
                if (smr.sharedMesh.vertexCount == 0)
                {
                    Debug.LogError($"VATValidator: Mesh on {smr.name} has 0 vertices.");
                    return false;
                }
            }
            return true;
        }

        public static bool ValidatePrefab(GameObject prefab)
        {
            if (prefab == null) return false;
            var mfs = prefab.GetComponentsInChildren<MeshFilter>(true);
            var mrs = prefab.GetComponentsInChildren<MeshRenderer>(true);
            bool valid = true;
            foreach (var mf in mfs)
            {
                if (mf.sharedMesh == null)
                {
                    Debug.LogError($"Prefab {prefab.name}: MeshFilter {mf.name} has no mesh.");
                    valid = false;
                }
            }
            foreach (var mr in mrs)
            {
                if (mr.sharedMaterials.Length == 0 || System.Array.TrueForAll(mr.sharedMaterials, m => m == null))
                {
                    Debug.LogError($"Prefab {prefab.name}: MeshRenderer {mr.name} has no valid materials.");
                    valid = false;
                }
            }
            var vatAnim = prefab.GetComponent<VATAnimator>();
            if (vatAnim != null)
            {
                if (vatAnim.animationDatabase == null && vatAnim.singleClipData == null)
                {
                    Debug.LogError($"Prefab {prefab.name}: VATAnimator has no database or single clip data.");
                    valid = false;
                }
            }
            return valid;
        }

        public static bool ValidateAgainstOriginal(GameObject originalPrefab, GameObject vatPrefab)
        {
            if (originalPrefab == null || vatPrefab == null) return false;

            var origMFs = originalPrefab.GetComponentsInChildren<MeshFilter>(true);
            var vatMFs = vatPrefab.GetComponentsInChildren<MeshFilter>(true);

            if (origMFs.Length != vatMFs.Length)
            {
                Debug.LogError($"MeshFilter count mismatch: original {origMFs.Length}, VAT {vatMFs.Length}");
                return false;
            }

            for (int i = 0; i < origMFs.Length; i++)
            {
                if (origMFs[i].sharedMesh != vatMFs[i].sharedMesh)
                {
                    // Boleh berbeda karena VAT mesh punya UV1 tambahan, tapi vertex count harus sama
                    if (origMFs[i].sharedMesh.vertexCount != vatMFs[i].sharedMesh.vertexCount)
                    {
                        Debug.LogError($"Vertex count mismatch on {origMFs[i].name}");
                        return false;
                    }
                }
            }

            var origMRs = originalPrefab.GetComponentsInChildren<MeshRenderer>(true);
            var vatMRs = vatPrefab.GetComponentsInChildren<MeshRenderer>(true);

            if (origMRs.Length != vatMRs.Length)
            {
                Debug.LogError($"MeshRenderer count mismatch");
                return false;
            }

            for (int i = 0; i < origMRs.Length; i++)
            {
                if (origMRs[i].sharedMaterials.Length != vatMRs[i].sharedMaterials.Length)
                {
                    Debug.LogError($"Material count mismatch on {origMRs[i].name}");
                    return false;
                }
            }

            return true;
        }
    }
}