using UnityEngine;

public class MusicTester : MonoBehaviour
{
    public MusicController musicController;

    [Header("Input Settings")]
    public KeyCode enterCombatKey = KeyCode.C;
    public KeyCode exitCombatKey = KeyCode.X;

    private void Start()
    {
        if (musicController == null)
        {
            musicController = FindFirstObjectByType<MusicController>();
        }
    }

    private void Update()
    {
        if (musicController == null) return;

        if (Input.GetKeyDown(enterCombatKey))
        {
            musicController.EnterCombat();
            Debug.Log("MusicTester: Enter Combat Triggered");
        }

        if (Input.GetKeyDown(exitCombatKey))
        {
            musicController.ExitCombat();
            Debug.Log("MusicTester: Exit Combat Triggered");
        }
    }

    private void OnGUI()
    {
        if (musicController == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 200, 100));
        
        if (GUILayout.Button($"Enter Combat ({enterCombatKey})"))
        {
            musicController.EnterCombat();
        }

        if (GUILayout.Button($"Exit Combat ({exitCombatKey})"))
        {
            musicController.ExitCombat();
        }

        GUILayout.EndArea();
    }
}
