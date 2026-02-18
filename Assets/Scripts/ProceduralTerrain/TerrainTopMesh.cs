using UnityEngine;


namespace ProceduralTerrain
{
    public class TerrainTopMesh : MonoBehaviour
    {
        public MeshFilter MeshFilter { get; private set; }

        void Awake() => MeshFilter = GetComponent<MeshFilter>();
    }
}


