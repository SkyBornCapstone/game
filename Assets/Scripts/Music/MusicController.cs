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
    [Range(0f, 1f)] public float musicVolume = 0.4f;
    [Range(0f, 1f)] public float outroVolumeScale = 0.24f;

    private AudioSource _sourceA;
    private AudioSource _sourceB;
    private bool _usingSourceA = true;
    private Coroutine _transitionCoroutine;
    private bool _inCombat = false;

    private void Awake()
    {
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
        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic, true, 0.01f);
        }
    }

    [ContextMenu("Enter Combat")]
    public void EnterCombat()
    {
        if (_inCombat) return;
        _inCombat = true;
        StartTransition(EnterCombatRoutine());
    }

    [ContextMenu("Exit Combat")]
    public void ExitCombat()
    {
        if (!_inCombat) return;
        _inCombat = false;
        StartTransition(ExitCombatRoutine());
    }

    private void StartTransition(IEnumerator routine)
    {
        if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
        _transitionCoroutine = StartCoroutine(routine);
    }

    private IEnumerator EnterCombatRoutine()
    {
        Debug.Log("MusicController: Entering Combat");
        AudioSource current = GetActive();
        AudioSource next = GetInactive();

        // 1. Schedule Intro
        double now = AudioSettings.dspTime + 0.05;
        double introDuration = (double)combatIntro.samples / combatIntro.frequency;
        
        next.clip = combatIntro;
        next.loop = false;
        next.PlayScheduled(now);

        // 2. Crossfade BGM -> Intro
        yield return CrossFade(current, next, crossFadeDuration, musicVolume);
        ToggleActive();

        // 3. Schedule Main Loop
        yield return new WaitForSeconds(crossFadeDuration + 0.1f); // Wait for fade to complete
        
        // Reuse the now-silent 'current' (which was BGM) for the Loop
        current.clip = combatMain;
        current.loop = true;
        current.PlayScheduled(now + introDuration);
        
        // Wait for Intro to finish
        double remaining = (now + introDuration) - AudioSettings.dspTime;
        if (remaining > 0) yield return new WaitForSeconds((float)remaining);

        // Ensure volume is up for the scheduled loop
        current.volume = musicVolume;
        ToggleActive(); // 'current' is active again
    }

    private IEnumerator ExitCombatRoutine()
    {
        Debug.Log("MusicController: Exiting Combat");
        AudioSource current = GetActive();
        AudioSource next = GetInactive();

        // 1. Play Outro
        next.clip = combatOutro;
        next.loop = true; // Loop for the "2x" requirement
        next.Play();

        // 2. Crossfade Loop -> Outro
        float outroTargetVol = musicVolume * outroVolumeScale;
        yield return CrossFade(current, next, outroCrossFadeDuration, outroTargetVol);
        ToggleActive();

        // 3. Wait for 2 Loops
        double singleLoop = (double)combatOutro.samples / combatOutro.frequency;
        // Wait until near end of 2nd loop to fade out.
        // We occupied 'outroCrossFadeDuration' already.
        double waitTime = (singleLoop * 2.0) - outroCrossFadeDuration - crossFadeDuration;
        
        if (waitTime > 0) yield return new WaitForSeconds((float)waitTime);

        if (!_inCombat)
        {
            // 4. Fade back to BGM
            current = GetActive(); // Playing Outro
            next = GetInactive();

            next.clip = backgroundMusic;
            next.loop = true;
            next.Play();
            
            yield return CrossFade(current, next, crossFadeDuration, musicVolume);
            ToggleActive();
        }
    }

    private void PlayMusic(AudioClip clip, bool loop, float fade)
    {
        AudioSource s = GetActive();
        s.clip = clip;
        s.loop = loop;
        s.volume = 0;
        s.Play();
        StartTransition(FadeTo(s, musicVolume, fade));
    }

    private IEnumerator CrossFade(AudioSource from, AudioSource to, float duration, float targetVol)
    {
        float t = 0;
        float startVol = from.volume;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            from.volume = Mathf.Lerp(startVol, 0, p);
            to.volume = Mathf.Lerp(0, targetVol, p);
            yield return null;
        }
        from.Stop();
        from.volume = 0;
        to.volume = targetVol;
    }

    private IEnumerator FadeTo(AudioSource source, float targetVol, float duration)
    {
        float t = 0;
        float startVol = source.volume;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, targetVol, t / duration);
            yield return null;
        }
        source.volume = targetVol;
    }

    private AudioSource GetActive() => _usingSourceA ? _sourceA : _sourceB;
    private AudioSource GetInactive() => _usingSourceA ? _sourceB : _sourceA;
    private void ToggleActive() => _usingSourceA = !_usingSourceA;
}
