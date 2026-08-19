using UnityEditor;
using UnityEngine;

namespace VATSystem
{
    public class VATMaterialFixer : EditorWindow
    {
        [MenuItem("Tools/VAT/Fix Transparent VAT Materials")]
        public static void FixTransparentMaterials()
        {
            string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/VAT" });
            int fixedCount = 0;
            foreach (string guid in matGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null || mat.shader.name != "VAT/VAT") continue;

                // Paksa alpha = 1
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.GetColor("_Color");
                    c.a = 1f;
                    mat.SetColor("_Color", c);
                }

                // Render queue ke Geometry
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

                // Matikan keyword transparan
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                EditorUtility.SetDirty(mat);
                fixedCount++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Fixed {fixedCount} transparent VAT materials.");
        }
    }
}