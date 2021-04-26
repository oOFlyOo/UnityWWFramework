using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WWFramework.Util
{
    public static class MeshUtil
    {
        private const int MaxResolution = 129;
        private const int ResolutionEdge = 1;
        
        private class QuadMeshData
        {
            private int _width;
            private int _height;
            
            private Vector3[] _vertices;
            private Vector2[] _uvs;
            private int[] _triangles;

            private int _triangleIndex;

            public QuadMeshData(int width, int height)
            {
                _width = width;
                _height = height;

                _vertices = new Vector3[width * height];
                _uvs = new Vector2[width * height];
                // 算出长宽下有多少个正方形，每一个正方形包含两个三角形，每两个三角形，包含6个顶点
                _triangles = new int[(width - 1) * (height - 1) * 6];

                _triangleIndex = 0;
            }

            public void AddVertex(int i, int j, float height, Vector3 meshScale)
            {
                var index = i + j * _width;
                _vertices[index] = Vector3.Scale(new Vector3(i, height, j), meshScale);
                _uvs[index] = new Vector2(i / (_width - 1f), j / (_height - 1f));
            }
            
            public void AddVertex(int i, int j, float height, Vector3 meshScale, Vector2 uv)
            {
                var index = i + j * _width;
                _vertices[index] = Vector3.Scale(new Vector3(i, height, j), meshScale);
                _uvs[index] = uv;
            }

            public void AddTriangle(int a, int b, int c)
            {
                _triangles[_triangleIndex] = a;
                _triangles[_triangleIndex + 1] = b;
                _triangles[_triangleIndex + 2] = c;

                _triangleIndex += 3;
            }

            public Mesh CreateMesh(IndexFormat format)
            {
                var mesh = new Mesh();
                mesh.indexFormat = format;

                mesh.vertices = _vertices;
                mesh.uv = _uvs;
                mesh.triangles = _triangles;
                
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                return mesh;
            }
        }

        /// <summary>
        /// 65536顶点数量限制
        /// </summary>
        /// <param name="data"></param>
        /// <param name="lod"></param>
        /// <returns></returns>
        private static int GetTerrainResolution(TerrainData data, IndexFormat format, int lod, int splitCount)
        {
            var resolution = data.heightmapResolution;
            resolution = GetResolutionLod(resolution,splitCount);
            if (format == IndexFormat.UInt16 && resolution > MaxResolution)
            {
                resolution = MaxResolution;
            }

            return GetResolutionLod(resolution, lod);
        }

        private static int GetResolutionLod(int resolution, int lod)
        {
            return ((resolution - ResolutionEdge) >> lod) + ResolutionEdge;
        }
        
        public static List<Mesh> ConvertTerrain2Mesh(Terrain terrain, IndexFormat format, int splitCount = 0, int lod = 0)
        {
            var terrainData = terrain.terrainData;
            var width = GetTerrainResolution(terrainData, format, lod, splitCount);
            var height = width;
            var size = terrainData.size;
            
            var tileGridNum = (int)Mathf.Pow(2, splitCount);
            var maxWidth = (width - 1) * tileGridNum + 1;
            var maxHeight = maxWidth;
            var meshScale = new Vector3(size.x / (width - 1) / tileGridNum, 1, size.z / (height - 1) / tileGridNum);

            // The function returns a two-dimensional array of size [yCount, xCount]
            var heights = terrainData.GetInterpolatedHeights(0, 0, maxWidth, maxHeight, 1f / (maxWidth - 1), 1f / (maxHeight - 1));

            var meshes = new List<Mesh>(tileGridNum * tileGridNum);

            for (int chunkI = 0; chunkI < tileGridNum; chunkI++)
            {
                for (int chunkJ = 0; chunkJ < tileGridNum; chunkJ++)
                {
                    var meshData = new QuadMeshData(width, height);

                    // 左下角开始，从下往上，从左往右添加顶点
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            // 共用一条边
                            var widthIndex = chunkI * (width - 1) + i;
                            var heightIndex = chunkJ * (height - 1) + j;
                            // meshData.AddVertex(i, j, heights[heightIndex, widthIndex], meshScale);
                            meshData.AddVertex(i, j, heights[heightIndex, widthIndex], meshScale, new Vector2(widthIndex / (maxWidth - 1f), heightIndex / (maxHeight - 1f)));
                        }
                    }

                    // 正方形顺序随顶点顺序，三角形顺时针方向为模型的正面
                    for (int i = 0; i < width - 1; i++)
                    {
                        for (int j = 0; j < height - 1; j++)
                        {
                            var a = i + j * width;
                            var b = i + (j + 1) * width;
                            var c = (i + 1) + (j + 1) * width;
                            var d = (i + 1) + j * width;
                    
                            meshData.AddTriangle(a, b, c);
                            meshData.AddTriangle(a, c, d);
                        }
                    }
                    
                    meshes.Add(meshData.CreateMesh(format));
                }
            }

            return meshes;
        }
    }
}