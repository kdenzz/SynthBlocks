using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class BlockRenderer : MonoBehaviour
    {
        [SerializeField] private Mesh blockMesh;
        [SerializeField] private Material blockMaterial;

        private readonly List<Matrix4x4> matrices = new();

        void Update()
        {
            if (blockMesh == null || blockMaterial == null || matrices.Count == 0)
            {
                return;
            }

            // Render in batches of 1023 for DrawMeshInstanced
            for (int i = 0; i < matrices.Count; i += 1023)
            {
                int count = Mathf.Min(1023, matrices.Count - i);
                Graphics.DrawMeshInstanced(blockMesh, 0, blockMaterial, matrices.GetRange(i, count));
            }
        }

        public void SetInstances(IReadOnlyList<Matrix4x4> instanceMatrices)
        {
            matrices.Clear();
            matrices.AddRange(instanceMatrices);
        }
    }
}


