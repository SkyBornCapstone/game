using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public static class TextureGenerator 
{
    public static void GenerateColors(Mesh mesh, float minHeight, float maxHeight, Gradient heightGradient, HashSet<int> egdeVertices)
    {
        Vector3[] oldVertices = mesh.vertices;
        int[] oldTriangles = mesh.triangles;
        
        Vector3[] newVertices = new Vector3[oldTriangles.Length];
        int[] newTriangles = new int[oldTriangles.Length];
        
        Color[] colors = new Color[oldTriangles.Length];
        Color[] oldColors = mesh.colors;
        for (int i = 0; i < oldTriangles.Length; i+=3)
        {
            Vector3 v1 =  oldVertices[oldTriangles[i]];
            Vector3 v2 = oldVertices[oldTriangles[i+1]];
            Vector3 v3 = oldVertices[oldTriangles[i+2]];
            
            float avgHeight = (v1.z + v2.z + v3.z) / 3;
            float t = Mathf.InverseLerp(minHeight, maxHeight, avgHeight);
            
            Color triangleColor = heightGradient.Evaluate(t);
            newVertices[i] = v1;
            newVertices[i + 1] = v2;
            newVertices[i + 2] = v3;
            Color col1 = triangleColor;
            Color col2 = triangleColor;
            Color col3 = triangleColor;



            colors[i] = col1;
            colors[i + 1] = col2;
            colors[i + 2] = col3;
            

            newTriangles[i] = i;
            newTriangles[i + 1] = i + 1;
            newTriangles[i + 2] = i + 2;
        }
        mesh.vertices = newVertices;
        mesh.triangles = newTriangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        
        
    }
}
