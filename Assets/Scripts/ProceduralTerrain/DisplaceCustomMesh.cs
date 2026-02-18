// using UnityEngine;
//
// public static class DisplaceCustomMesh
// {
//     public static void DisplaceCustomMesh(MeshFilter targetMeshFilter, float heightMultiplier, AnimationCurve heightCurve) {
//         Mesh mesh = targetMeshFilter.sharedMesh;
//         Vector3[] vertices = mesh.vertices;
//     
//         // We need to generate or access noise. 
//         // Since your Noise class returns a 2D array, it's actually easier 
//         // to call a single point of noise for each vertex.
//     
//         for (int i = 0; i < vertices.Length; i++) {
//             // Use the vertex's local or world position for the noise sample
//             float sampleX = (vertices[i].x + offset.x) / noiseScale;
//             float sampleZ = (vertices[i].z + offset.y) / noiseScale;
//
//             // Reuse your noise logic (simplified here for clarity)
//             float noiseHeight = GetNoiseValueAtPoint(sampleX, sampleZ); 
//         
//             // Apply the height
//             vertices[i].y = heightCurve.Evaluate(noiseHeight) * heightMultiplier;
//         }
//
//         mesh.vertices = vertices;
//         mesh.RecalculateNormals();
//         mesh.RecalculateBounds();
//     }
// }
