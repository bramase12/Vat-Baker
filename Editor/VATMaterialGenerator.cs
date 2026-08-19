using UnityEditor;
using UnityEngine;

namespace VATSystem
{
    public static class VATMaterialGenerator
    {
        private const string ShaderName = "VAT/VAT";

        public static Material CreateVATMaterial(Material original, VATAnimationData animData)
        {
            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogError($"VAT shader '{ShaderName}' not found.");
                return null;
            }

            if (animData == null || animData.positionTexture == null || animData.normalTexture == null)
            {
                Debug.LogError("VATAnimationData missing required textures.");
                return null;
            }

            Material vat = new Material(shader);
            vat.name = (original ? original.name : "Material") + "_VAT";

            if (original != null)
            {
                vat.CopyPropertiesFromMaterial(original);
                foreach (var kw in original.shaderKeywords)
                    vat.EnableKeyword(kw);

                // 🔥 Paksa alpha penuh dan render queue Geometry (opaque)
                if (vat.HasProperty("_Color"))
                {
                    Color c = vat.GetColor("_Color");
                    c.a = 1f;
                    vat.SetColor("_Color", c);
                }
                vat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

                // Matikan mode transparan yang mungkin terbawa
                vat.DisableKeyword("_ALPHABLEND_ON");
                vat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                // Biarkan _ALPHATEST_ON jika material asli memilikinya (untuk cutout seperti rambut)
            }

            // Data VAT
            vat.SetTexture("_PosTex", animData.positionTexture);
            vat.SetTexture("_NrmTex", animData.normalTexture);
            if (animData.tangentTexture != null)
                vat.SetTexture("_TanTex", animData.tangentTexture);
            vat.SetFloat("_VAT_VertexCount", animData.vertexCount);
            vat.SetFloat("_VAT_TotalFrames", animData.totalFrames);
            vat.SetFloat("_VAT_TexWidth", animData.textureWidth);
            vat.SetFloat("_VAT_RowsPerFrame", animData.rowsPerFrame);

            return vat;
        }
    }
}