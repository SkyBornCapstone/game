using System.Collections;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip combatIntro;
    public AudioClip combatMain;
    public AudioClip combatOutro;

    [Header("Settings")]
    public float crossFadeDuration = 1.0f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    private AudioSource _sourceA;
    private AudioSource _sourceB;
    private bool _usingSourceA = true;
    private Coroutine _activeCoroutine;
    private bool _inCombat = false;

    private void Awake()
    {
        // Create two audio sources for crossfading
        _sourceA = gameObject.AddComponent<AudioSource>();
        _sourceB = gameObject.AddComponent<AudioSource>();

        ConfigureSource(_sourceA);
        ConfigureSource(_sourceB);
    }

    private void ConfigureSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = false;
        source.volume = 0f;
    }

    private void Start()
    {
        // Start background music
        if (backgroundMusic != null)
        {
            PlayClip(backgroundMusic, true, 0f, true);
        }
    }

    [ContextMenu("Enter Combat")]
    public void EnterCombat()
    {
        if (_inCombat) return; // Already in combat
        _inCombat = true;

        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(EnterCombatRoutine());
    }

    [ContextMenu("Exit Combat")]
    public void ExitCombat()
    {
        if (!_inCombat) return; // Not in combat
        _inCombat = false;

        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(ExitCombatRoutine());
    }

    private IEnumerator EnterCombatRoutine()
    {
        Debug.Log("MusicController: Entering Combat Routine");
        // 1. Fade out current BGM while fading in Intro
        // We'll play Intro on the free source
        AudioSource activeSource = GetActiveSource();
        AudioSource nextSource = GetInactiveSource();
        
        // Play intro (not looping)
        nextSource.clip = combatIntro;
        nextSource.loop = false;
        nextSource.Play();
        
        // Crossfade
        yield return StartCoroutine(CrossFade(activeSource, nextSource, crossFadeDuration));
        
        // Toggle active source tracking
        ToggleActiveSource();

        // 2. Wait for intro to finish (minus a small buffer if needed? no, seamlessly)
        // Actually, for seamless intro->loop, playScheduled is best, but let's try simple wait first.
        double duration = (double)combatIntro.samples / combatIntro.frequency;
        // We already waited crossFadeDuration. 
        // Remaining time:
        double remaining = duration - crossFadeDuration;
        
        if (remaining > 0)
            yield return new WaitForSeconds((float)remaining);

        // 3. Play Combat Loop
        // We can just switch the clip on the NOW active source (which finished the intro)
        // But to be perfectly seamless, we might want to schedule it.
        // Let's keep it simple: Play the Loop on the SAME source immediately.
        // Or if we want to crossfade? User didn't ask for crossfade INTRO->LOOP, usually it's a direct cut.
        
        if (_inCombat) // Check if we are still in combat
        {
            activeSource = GetActiveSource(); // This is the one that played the intro
            activeSource.clip = combatMain;
            activeSource.loop = true;
            activeSource.Play();
        }
    }

    private IEnumerator ExitCombatRoutine()
    {
        // 1. Fade to combat outro
        AudioSource activeSource = GetActiveSource();
        AudioSource nextSource = GetInactiveSource();

        nextSource.clip = combatOutro;
        nextSource.loop = false;
        nextSource.Play();

        // Crossfade from Combat Loop -> Outro
        yield return StartCoroutine(CrossFade(activeSource, nextSource, crossFadeDuration));
        ToggleActiveSource();

        // 2. Wait for outro to finish
        double duration = (double)combatOutro.samples / combatOutro.frequency;
        double remaining = duration - crossFadeDuration;
        
        if (remaining > 0)
            yield return new WaitForSeconds((float)remaining);

        // 3. Fade back to start of looping background music
        if (!_inCombat) // Ensure we haven't re-entered combat
        {
            activeSource = GetActiveSource(); // This is the one playing Outro (finishing now)
            nextSource = GetInactiveSource();

            nextSource.clip = backgroundMusic;
            nextSource.loop = true;
            nextSource.Play(); // Starts from beginning

            // "Fade back to start" - imply crossfade? User said "Fade back to"
            yield return StartCoroutine(CrossFade(activeSource, nextSource, crossFadeDuration));
            ToggleActiveSource();
        }
    }
    
    // Helper to play a specific clip (used for initial BGM)
    private void PlayClip(AudioClip clip, bool loop, float fadeTime, bool asNewTrack)
    {
        AudioSource source = _usingSourceA ? _sourceA : _sourceB;
        source.clip = clip;
        source.loop = loop;
        source.volume = 0; // Start silent for fade in
        source.Play();
        
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(FadeIn(source, fadeTime > 0 ? fadeTime : 0.01f));
    }

    private AudioSource GetActiveSource() => _usingSourceA ? _sourceA : _sourceB;
    private AudioSource GetInactiveSource() => _usingSourceA ? _sourceB : _sourceA;
    private void ToggleActiveSource() => _usingSourceA = !_usingSourceA;

    private IEnumerator CrossFade(AudioSource from, AudioSource to, float duration)
    {
        float timer = 0f;
        float startVolFrom = from.volume;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            
            from.volume = Mathf.Lerp(startVolFrom, 0f, t);
            to.volume = Mathf.Lerp(0f, musicVolume, t);
            
            yield return null;
        }
        
        Debug.Log($"MusicController: CrossFade Finished. Stopping {from.clip.name}.");
        from.volume = 0f;
        from.Stop();
        to.volume = musicVolume;
    }

    private IEnumerator FadeIn(AudioSource source, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, musicVolume, timer / duration);
            yield return null;
        }
        source.volume = musicVolume;
    }
}
