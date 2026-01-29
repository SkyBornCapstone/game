using UnityEngine;
using UnityEditor;

public class MusicSetupTool
{
    [MenuItem("Tools/Setup Music Controller")]
    public static void Setup()
    {
        // 1. Find or Create GameObject
        MusicController controller = Object.FindAnyObjectByType<MusicController>();
        if (controller == null)
        {
            GameObject go = new GameObject("MusicController");
            controller = go.AddComponent<MusicController>();
            Undo.RegisterCreatedObjectUndo(go, "Create MusicController");
        }
        else
        {
            Undo.RecordObject(controller, "Setup MusicController");
        }

        // 2. Load Assets
        controller.backgroundMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/FullBackgroundTrack.aif");
        controller.combatIntro = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/combat_intro.aif");
        controller.combatMain = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/combat_main.aif");
        controller.combatOutro = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/combat_outro.aif");

        // 3. Set Settings
        controller.crossFadeDuration = 1.0f;
        controller.musicVolume = 0.5f;

        // 4. Validate
        if (controller.backgroundMusic == null) Debug.LogError("Could not find FullBackgroundTrack.aif");
        if (controller.combatIntro == null) Debug.LogError("Could not find combat_intro.aif");
        if (controller.combatMain == null) Debug.LogError("Could not find combat_main.aif");
        if (controller.combatOutro == null) Debug.LogError("Could not find combat_outro.aif");

        EditorUtility.SetDirty(controller);
        Debug.Log("Music Controller Setup Complete!");
    }
}
