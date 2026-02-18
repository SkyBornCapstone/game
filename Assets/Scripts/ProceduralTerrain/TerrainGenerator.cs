using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Terrain;
using Unity.VisualScripting;

namespace ProceduralTerrain
{
    public class TerrainGenerator : MonoBehaviour
    {



        public void GenerateMap(TerrainNoiseData terrainData, GameObject parentObject)
        {

            System.Random prng = new System.Random(terrainData.seed);
            Vector2[] octaveOffsets = new Vector2[terrainData.octaves];
            for (int i = 0; i < terrainData.octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + terrainData.offset.x;
                float offsetY = prng.Next(-100000, 100000) + terrainData.offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            Transform parent = parentObject.transform;
            // print(parentObject.childrean);
            foreach (Transform child in parent)
            {
                // print(child.name);
                if (child.name == "islands_large__top_1")
                {
                    TerrainTopMesh component = child.GetComponent<TerrainTopMesh>();
                    print(component.GetType().Name);

                }
            }

            TerrainTopMesh top = parent.GetComponentInChildren<TerrainTopMesh>(true);
            print(top);
            MeshFilter topMeshFilter = top.MeshFilter;
            string terrainName = topMeshFilter.name + "Terrain";

            Transform existingTerrain = parent.Find(terrainName);

            GameObject terrainObject;
            MeshFilter newMeshFilter;
            MeshRenderer newMeshRenderer;
            MeshCollider meshCollider;
            if (existingTerrain != null)
            {
                terrainObject = existingTerrain.gameObject;
                newMeshFilter = existingTerrain.GetComponent<MeshFilter>();
                newMeshRenderer = existingTerrain.GetComponent<MeshRenderer>();
                meshCollider = existingTerrain.GetComponent<MeshCollider>();
            }
            else
            {
                terrainObject = new GameObject(terrainName);
                terrainObject.transform.SetParent(parent);
                terrainObject.transform.position = topMeshFilter.transform.position;
                terrainObject.transform.rotation = topMeshFilter.transform.rotation;
                terrainObject.transform.localScale = topMeshFilter.transform.localScale;
                newMeshFilter = terrainObject.AddComponent<MeshFilter>();
                newMeshRenderer = terrainObject.AddComponent<MeshRenderer>();
                meshCollider = terrainObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = newMeshFilter.sharedMesh;
            }



            newMeshRenderer.sharedMaterial = terrainData.material;
            Mesh mesh = Instantiate(topMeshFilter.sharedMesh);
            mesh.name = terrainName + " Mesh";
            newMeshFilter.mesh = mesh;
            Vector3[] originalvertices = mesh.vertices;
            Vector3[] modifiedvertices = new Vector3[originalvertices.Length];

            Color[] colors = topMeshFilter.sharedMesh.colors;

            Bounds bounds = mesh.bounds;


            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            for (int i = 0; i < originalvertices.Length; i++)
            {
                Vector3 v = originalvertices[i];
                float height = Noise.GenerateHeight(v.x * terrainData.noiseScale, v.y * terrainData.noiseScale,
                    terrainData.persistence, terrainData.lacunarity, terrainData.heightMeshMultiplier,
                    terrainData.octaves, octaveOffsets);

                float finalHeight = terrainData.heightCurve.Evaluate(height) * terrainData.heightMeshMultiplier;
                if (colors[i].r < 1.0f)
                {
                    finalHeight = 0f;
                }

                modifiedvertices[i] = new Vector3(v.x, v.y, finalHeight);
                if (finalHeight < minHeight)
                {
                    minHeight = finalHeight;
                }
                else if (finalHeight > maxHeight)
                {
                    maxHeight = finalHeight;
                }
            }

            mesh.vertices = modifiedvertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
            TextureGenerator.GenerateColors(mesh, minHeight, maxHeight, terrainData.colorGradient);


        }


        // void OnValidate()
        // {
        //
        //
        //     if (lacunarity < 1)
        //     {
        //         lacunarity = 1;
        //     }
        //
        //     if (octaves < 0)
        //     {
        //         octaves = 0;
        //     }
        //
        // }


        [System.Serializable]
        public struct TerrainType
        {
            public string name;
            public float height;

            public Color colour;
        }
    }
}