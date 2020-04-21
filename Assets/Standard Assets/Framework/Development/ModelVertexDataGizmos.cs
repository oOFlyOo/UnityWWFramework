

using UnityEngine;

namespace WWFramework.Development
{
    [ExecuteInEditMode]
    public class ModelVertexDataGizmos: MonoBehaviour
    {
        [SerializeField]
        private float _length = 0.1f;
        [SerializeField]
        private Color _color = Color.red;

        private Mesh _shareMesh;

        private void InitModelData()
        {
            var meshFilter = GetComponent<MeshFilter>();
            _shareMesh = meshFilter ? meshFilter.sharedMesh : GetComponent<SkinnedMeshRenderer>().sharedMesh;
        }

        private void OnDrawGizmos()
        {
            if (_shareMesh == null)
            {
                InitModelData();

                return;
            }

            var localToWorld = transform.localToWorldMatrix;
            var transpose = localToWorld.inverse.transpose;

            DrawVectors(_shareMesh.vertices, _shareMesh.normals, ref localToWorld, ref transpose, ref _color, _length);
        }

        private void DrawVectors(Vector3[] vertices, Vector3[] vectors, ref Matrix4x4 vertexMatrix,
            ref Matrix4x4 vectorMatrix, ref Color color, float length)
        {
            Gizmos.color = color;

            for (int i = 0; i < vertices.Length; i++)
            {
                var worldVertex = vertexMatrix.MultiplyPoint(vertices[i]);
                var worldVector = vectorMatrix.MultiplyVector(vectors[i]);
                worldVector.Normalize();

                Gizmos.DrawLine(worldVertex, worldVertex + worldVector * length);
            }
        }
    }
}