using System.Collections.Generic;
using UnityEngine;

namespace VATSystem
{
    public class VATMeshBaker
    {
        private readonly SkinnedMeshRenderer smr;
        private readonly Mesh bakeMesh;
        private readonly int vertexCount;

        public int VertexCount => vertexCount;
        public SkinnedMeshRenderer SkinnedMeshRenderer => smr;

        public VATMeshBaker(SkinnedMeshRenderer smr)
        {
            this.smr = smr;
            var sharedMesh = smr.sharedMesh;
            vertexCount = sharedMesh.vertexCount;
            bakeMesh = new Mesh();
            bakeMesh.indexFormat = sharedMesh.indexFormat;
        }

        public void BakeFrame(List<Vector3> positions, List<Vector3> normals, List<Vector4> tangents, bool recordTangents)
        {
            smr.BakeMesh(bakeMesh);
            positions.AddRange(bakeMesh.vertices);
            normals.AddRange(bakeMesh.normals);
            if (recordTangents)
            {
                if (bakeMesh.tangents.Length > 0)
                    tangents.AddRange(bakeMesh.tangents);
                else
                {
                    var dummyTangent = new Vector4(1, 0, 0, 1);
                    for (int i = 0; i < bakeMesh.vertexCount; i++)
                        tangents.Add(dummyTangent);
                }
            }
        }

        /// <summary>
        /// Membuat salinan persis mesh asli dengan tambahan UV1 untuk VAT lookup.
        /// Tidak mengubah vertex order, topology, UV0, normal, atau tangent.
        /// </summary>
        public Mesh CreateStaticVATMesh(int vertexOffset, int totalVertexCount)
        {
            var mesh = Object.Instantiate(smr.sharedMesh);
            mesh.name = smr.sharedMesh.name + "_VAT";
            int vc = mesh.vertexCount;
            var uv1 = new Vector2[vc];
            float inv = 1f / totalVertexCount;
            for (int i = 0; i < vc; i++)
                uv1[i] = new Vector2((i + vertexOffset + 0.5f) * inv, 0f);
            mesh.SetUVs(1, uv1); // channel 1 = uv2
            mesh.UploadMeshData(false);
            return mesh;
        }

        public void Dispose()
        {
            Object.DestroyImmediate(bakeMesh);
        }
    }
}