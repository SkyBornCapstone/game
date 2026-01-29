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
    public float outroCrossFadeDuration = 3.0f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float outroVolumeScale = 0.7f;

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
        
        // 1. Identify sources
        AudioSource bgmSource = GetActiveSource();
        AudioSource introSource = GetInactiveSource();
        // We will reuse the bgmSource for the Loop after it fades out
        
        // Calculate precise timings
        double introDuration = (double)combatIntro.samples / combatIntro.frequency;
        double now = AudioSettings.dspTime + 0.05; // Small buffer for scheduling
        double loopStartTime = now + introDuration;

        // 2. Schedule Intro (on inactive source)
        introSource.clip = combatIntro;
        introSource.loop = false;
        introSource.PlayScheduled(now);

        // 3. Schedule Loop (on CURRENT source, which will fade out first)
        // We need to commit the clip and loop setting ahead of time, which is tricky 
        // because it's currently playing BGM.
        // Actually, we can't swap the clip on bgmSource until it stops playing BGM.
        // If PlayScheduled is called on a source, it queues? No, it replaces if playing?
        // Wait, AudioSource can only have ONE clip. Changing .clip stops the current one?
        // Yes, changing .clip usually stops play unless we are careful?
        // Unity allows setting clip while playing? No, usually swaps.
        
        // BETTER APPROACH:
        // We ideally need THREE sources for perfect crossfade + scheduled gapless.
        // Source A: BGM (Fading Out)
        // Source B: Intro (Playing)
        // Source A: Loop (Waiting to Play) -> WE CANNOT DO THIS simultaneously on Source A.
        
        // If BGM fades out in 1s, and Intro is 3s.
        // Source A is silent from t=1s to t=3s.
        // We can swap clip on Source A at t=1.1s (after fade).
        // Then Schedule Play at t=3s.
        
        // Start Crossfade Logic (handled manually to avoid conflict)
        StartCoroutine(CrossFade(bgmSource, introSource, crossFadeDuration, musicVolume));
        
        // We switch "Active" to IntroSource for now so logic holds
        ToggleActiveSource(); 
        
        // Wait until BGM fade is definitely done
        yield return new WaitForSeconds(crossFadeDuration + 0.1f);
        
        // Now bgmSource (old A) is silent and free.
        bgmSource.clip = combatMain;
        bgmSource.loop = true;
        // Schedule it to start exactly when Intro ends
        bgmSource.PlayScheduled(loopStartTime);
        
        // Wait for the rest of the intro
        double remaining = loopStartTime - AudioSettings.dspTime;
        if (remaining > 0)
            yield return new WaitForSeconds((float)remaining);
            
        // At this point, Intro finishes, Loop starts on bgmSource.
        // We need to ensure bgmSource volume is UP.
        // The CrossFade faded it to 0.
        bgmSource.volume = musicVolume;
        
        // Now the "Active" source is bgmSource (Loop) again!
        // We toggled once before (A->B). Now we need to toggle back (B->A).
        ToggleActiveSource();
    }

    private IEnumerator ExitCombatRoutine()
    {
        Debug.Log("MusicController: Exiting Combat Routine");
        
        // 1. Fade to combat outro
        AudioSource activeSource = GetActiveSource();
        AudioSource nextSource = GetInactiveSource();

        nextSource.clip = combatOutro;
        // User wants it looped twice. We can set loop=true and manage timing manually.
        nextSource.loop = true; 
        nextSource.Play();

        // Crossfade from Combat Loop -> Outro using longer duration
        yield return StartCoroutine(CrossFade(activeSource, nextSource, outroCrossFadeDuration, musicVolume * outroVolumeScale));
        ToggleActiveSource();

        // 2. Wait for outro to loop twice
        // We want to hear the outro twice.
        // Total duration we want to occupy is 2 * ClipLength.
        // However, we just spent 'outroCrossFadeDuration' transition into it.
        // We should wait until we are near the end of the 2nd loop to start fading to BGM.
        
        double outroDuration = (double)combatOutro.samples / combatOutro.frequency;
        double totalOutroPlayTime = outroDuration * 2.0;
        
        // We start fading OUT of the outro slightly before it ends the 2nd time, 
        // to crossfade into BGM? Or fade BGM in while Outro finishes?
        // "fade back to the start of the looping background music"
        // Let's assume standard crossfade duration for Outro -> BGM.
        
        // Time to wait = TotalPlayTime - (TimeAlreadySpentInFadeIn + TimeNeededForFadeOutToBGM)
        // Wait, 'TimeAlreadySpentInFadeIn' (outroCrossFadeDuration) was while Outro was playing.
        // So we are at t = outroCrossFadeDuration.
        // We want to start next fade at t = totalOutroPlayTime - crossFadeDuration (standard fade).
        
        double timeToWait = totalOutroPlayTime - outroCrossFadeDuration - crossFadeDuration;
        
        if (timeToWait > 0)
            yield return new WaitForSeconds((float)timeToWait);

        // 3. Fade back to start of looping background music
        if (!_inCombat) // Ensure we haven't re-entered combat
        {
            activeSource = GetActiveSource(); // Currently playing Outro
            nextSource = GetInactiveSource();

            nextSource.clip = backgroundMusic;
            nextSource.loop = true;
            nextSource.Play(); // Starts from beginning

            yield return StartCoroutine(CrossFade(activeSource, nextSource, crossFadeDuration, musicVolume));
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

    private IEnumerator CrossFade(AudioSource from, AudioSource to, float duration, float targetVolume)
    {
        float timer = 0f;
        float startVolFrom = from.volume;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            
            from.volume = Mathf.Lerp(startVolFrom, 0f, t);
            to.volume = Mathf.Lerp(0f, targetVolume, t);
            
            yield return null;
        }
        
        Debug.Log($"MusicController: CrossFade Finished. Stopping {from.clip.name}.");
        from.volume = 0f;
        from.Stop();
        to.volume = targetVolume;
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
