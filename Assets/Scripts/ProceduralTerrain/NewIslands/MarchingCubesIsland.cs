using UnityEngine;
using core;

public class MarchingCubesIsland : MonoBehaviour
{
    [Header("Grid")]
    public int resX = 32;
    public int resY = 32;
    public int resZ = 32;
    public float cellSize = 1f;
    public float isoLevel = 0f;

    [Header("Preset")]
    public IslandPreset preset = IslandPreset.Custom;

    [Header("Island Settings")]
    public IslandDensitySettings densitySettings;
    public Material islandMaterial;

    [Header("Gradient")]
    public Gradient islandGradient = new Gradient();

    [Header("Editor")]
    public bool autoUpdate = false;

    private GameObject _islandObject;

    public void Generate()
    {
        if (_islandObject != null)
            DestroyImmediate(_islandObject);

        DensityField densityField = new DensityField(resX, resY, resZ);
        densityField.PopulateIsland(densitySettings);
        densityField.PadBorders(1f);

        Mesh mesh = MeshBuilder.Build(densityField.Values, isoLevel, cellSize, islandGradient);

        _islandObject = new GameObject("Island_Mesh");
        _islandObject.transform.position = Vector3.zero;
        _islandObject.transform.SetParent(this.transform);

        var meshFilter              = _islandObject.AddComponent<MeshFilter>();
        var meshRenderer            = _islandObject.AddComponent<MeshRenderer>();
        meshFilter.sharedMesh       = mesh;
        meshRenderer.sharedMaterial = islandMaterial;
    }

    // Loads preset values into densitySettings so you can see and edit them.
    public void LoadPreset()
    {
        if (IslandPresetLibrary.TryApply(preset, out IslandDensitySettings s))
            densitySettings = s;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (autoUpdate && !Application.isPlaying)
            UnityEditor.EditorApplication.delayCall += () => { if (this) Generate(); };
    }
#endif
}