using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAudioManager
{
    float SfxVolumeScale { get; set; }

    /// <summary>
    /// Play/Pause the BGM depending on whether value is True/False
    /// </summary>
    /// <param name="value">Whether to Play/Pause the BGM</param>
    void SetBgmPlaying(bool value);
    void RestartBgm();

    void PlayLevelStartBgm();

    void GameOver();

    void CastFire(AudioSource src = null);
    void FireHits(AudioSource src = null);
    void FireCharged(AudioSource src = null);
    void LoopFire(AudioSource src = null);
    void DismissFire(AudioSource src = null);

    void CastIce(AudioSource src = null);
    void IceHits(AudioSource src = null);
    void IceCharged(AudioSource src = null);
    void LoopIce(AudioSource src = null);
    void DismissIce(AudioSource src = null);

    void DismissSpell(AudioSource src = null);

    // Beta Stuff
    void ClickButton();

    void AttackObelisk(AudioSource src = null);
    void ObeliskDeathSound(AudioSource src = null);

    void AttackShield(AudioSource src = null);

    void CastGrav(AudioSource src = null);
    void GravHits(AudioSource src = null);

    void EnemyBomberExplosion(AudioSource src = null);
    void EnemyDeflectSpell(AudioSource src = null);
    void EnemyDeathScream(AudioSource src = null);

    void MeteorFall(AudioSource src = null);
}

[System.Serializable]
public class AudioSettings
{
    public AudioClip tutorial;
    public AudioClip bgm;
    public AudioClip gameOver;

    public AudioClip fireCast; // Gesture throw fire
    public AudioClip fireImpact; // Fire hits target
    public AudioClip fireFull; // Fire is fully charged

    public AudioClip iceCast; // Gesture throw ice
    public AudioClip iceImpact; // Ice hits target
    public AudioClip iceFull; // Ice is fully charged

    public AudioClip gravCast; // Gesture throw grav ball
    public AudioClip gravImpact; // Grav ball activates

    public AudioClip fireLoop; // Looping sound for when holding fire
    public AudioClip iceLoop; // placeholder


    // Dismiss sounds, fire and ice are placeholders
    public AudioClip spellDismiss;
    public AudioClip fireDismiss;
    public AudioClip iceDismiss;

    // Enemy attacks
    public AudioClip attackObelisk;
    public AudioClip attackShield;
    public AudioClip enemyBomberExplosion;
    public AudioClip enemyShieldDeflect;

    // Destruction
    public AudioClip obeliskDeath;
    public AudioClip enemyScream;

    // UI Sounds
    public AudioClip buttonClick;

    // MeteorFall
    public AudioClip meteorLoop;
    public AudioClip meteorExplode1;
    public AudioClip meteorExplode2;
    public AudioClip meteorExplode3;
    public AudioClip meteorWhistle;
}

public class AudioManager : MonoBehaviour, IAudioManager
{
    private AudioSource bgm;
    private AudioSource oneShotAudioSource;

    private float _sfxVolumeScale = 1;

    public float SfxVolumeScale
    {
        get { return _sfxVolumeScale; }
        set {
            Debug.Log("SFX Volume: " + value);
            _sfxVolumeScale = value; 
        }
    }


    private AudioSettings settings;

    public void Initialize(AudioSettings audioSettings)
    {
        settings = audioSettings;
    }

    #region IAudioManager
    public void SetBgmPlaying(bool value)
    {
        if (value && !bgm.isPlaying)
        {
            bgm.UnPause();
        }
        else if (!value && bgm.isPlaying)
        {
            bgm.Pause();
        }
    }

    public void RestartBgm()
    {
        bgm.Stop();
        bgm.Play();
    }

    public void PlayLevelStartBgm()
    {
        bgm.Stop();
        bgm.clip = settings.bgm;
        bgm.Play();
    }

    public void GameOver()
    {
        PlayOneShot(settings.gameOver);
    }

    public void CastFire(AudioSource src = null)
    {
        PlayOneShot(settings.fireCast, src);
    }

    public void FireHits(AudioSource src = null)
    {
        PlayOneShot(settings.fireImpact, src);
    }

    public void FireCharged(AudioSource src = null)
    {
        PlayOneShot(settings.fireFull, src);
    }

    public void LoopFire(AudioSource src = null)
    {
        PlayOneShot(settings.fireLoop, src);
    }

    public void DismissFire(AudioSource src = null)
    {
        PlayOneShot(settings.fireDismiss, src);
    }

    public void CastIce(AudioSource src = null)
    {
        PlayOneShot(settings.iceCast, src);
    }

    public void IceHits(AudioSource src = null)
    {
        PlayOneShot(settings.iceImpact, src);
    }

    public void IceCharged(AudioSource src = null)
    {
        PlayOneShot(settings.iceFull, src);
    }

    public void LoopIce(AudioSource src = null)
    {
        PlayOneShot(settings.iceLoop, src);
    }

    public void DismissIce(AudioSource src = null)
    {
        PlayOneShot(settings.iceDismiss, src);
    }

    public void DismissSpell(AudioSource src = null)
    {
        PlayOneShot(settings.spellDismiss, src);
    }

    // Beta Stuff
    public void ClickButton()
    {
        PlayOneShot(settings.buttonClick);
    }

    public void AttackObelisk(AudioSource src = null)
    {
        PlayOneShot(settings.attackObelisk, src);
    }

    public void ObeliskDeathSound(AudioSource src = null)
    {
        PlayOneShot(settings.obeliskDeath, src);
    }

    public void AttackShield(AudioSource src = null)
    {
        PlayOneShot(settings.attackShield, src);
    }

    public void CastGrav(AudioSource src = null)
    {
        PlayOneShot(settings.gravCast, src);
    }

    public void GravHits(AudioSource src = null)
    {
        PlayOneShot(settings.gravImpact, src);
    }

    public void EnemyBomberExplosion(AudioSource src = null)
    {
        PlayOneShot(settings.enemyBomberExplosion, src);
    }

    public void EnemyDeflectSpell(AudioSource src = null)
    {
        PlayOneShot(settings.enemyShieldDeflect, src);
    }

    public void EnemyDeathScream(AudioSource src = null)
    {
        PlayOneShot(settings.enemyScream, src);
    }

    public void MeteorFall(AudioSource src = null)
    {
        PlayOneShot(settings.meteorLoop, src);
    }

    #endregion

    #region Unity Events
    private void Awake()
    {
        bgm = CreateAudioSource("BGM");
        AudioSource src = CreateAudioSource("OneShotAudioSource");
        oneShotAudioSource = src;
    }

    private void Start()
    {
        if (settings == null)
        {
            Debug.LogError("DESTROYING SELF.  AudioSettings were not initialized.");
            Destroy(this);
            return;
        }

        if (settings.bgm == null)
        {
            Debug.LogError("BGM clip is null.");
        }

        bgm.clip = settings.tutorial;
        bgm.loop = true;
        bgm.Play();
    }
    #endregion

    #region Helper Functions
    private bool PlayOneShot(AudioClip clip, AudioSource src = null)
    {
        if (clip == null)
        {
            Debug.LogError("Attempted to play a null audio clip.");
            return false;
        }

        if (src == null)
        {
            src = oneShotAudioSource;
        }
        src.PlayOneShot(clip, SfxVolumeScale);
        return true;
    }

    private AudioSource CreateAudioSource(string name)
    {
        GameObject g = new GameObject(name);
        g.transform.parent = transform;
        AudioSource src = g.AddComponent<AudioSource>();
        src.playOnAwake = false;
        return src;
    }


    #endregion
}
