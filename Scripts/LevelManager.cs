using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager singleton;

    public Level currentLevel = Level.Start;
    public GameObject menuObject;
    public GameObject god;
    public GameObject shield;
    public GameObject waveManager;
    public GameObject tutorialFire;
    public GameObject tutorialIce;
    public GameObject tutorialGravity;
    public GameObject tutorialTargetFire;
    public GameObject tutorialTargetIce;
    public GameObject tutorialTargetGravity;

    public Ending ending;

    public AudioSource audioSource;

    public List<AudioClip> lookAt;
    public List<AudioClip> tutFire;
    public List<AudioClip> tutIce;
    public List<AudioClip> tutSpecial;
    public List<AudioClip> tutAttackGod;
    public List<AudioClip> tutEnd;
    public List<AudioClip> level1;
    public List<AudioClip> level2;
    public List<AudioClip> level3;
    public List<AudioClip> level4;
    public List<AudioClip> end;

    public List<AudioClip> obelisk1down;
    public List<AudioClip> obelisk2down;

    public List<AudioClip> gameOver;

    private List<AudioClip> currentAudioClips;
    private int currentClipIndex = 0;
    private int obelisksDown = 0;


    public enum Level
    {
        Start,
        TutLookAt,
        TutFire,
        TutIce,
        TutSpecial,
        TutAttackGod,
        Level1,
        Level2,
        Level3,
        Level4,
        End
    };

    void Start()
    {
        LevelManager.singleton = this;
        Progress(Level.Start);
    }

    void Update()
    {
        // manual overrides
        switch(Input.inputString)
        {
            case "0":
            case "1":
            case "2":
            case "3":
            case "4":
            case "5":
            case "6":
            case "7":
            case "8":
            case "9":
                Progress((Level)int.Parse(Input.inputString));
                Debug.Log("Manually switching to " + currentLevel);
                break;
            case "-":
                Progress((Level)10);
                Debug.Log("Manually switching to " + currentLevel);
                break;
            default:
                break;
        }

        // progress audio if the clip has ended
        if (this.currentAudioClips != null && this.currentClipIndex < this.currentAudioClips.Count-1 && !this.audioSource.isPlaying)
        {
            this.currentClipIndex++;
            this.audioSource.enabled = false;
            this.audioSource.clip = (this.currentAudioClips[this.currentClipIndex]);
            this.audioSource.enabled = true;
            GameManager.S.Audio.SfxVolumeScale = 0.5F;
        } 
        // if last clip ended, restore volume scale
        else if (this.currentAudioClips != null && this.currentClipIndex == this.currentAudioClips.Count - 1 && !this.audioSource.isPlaying && GameManager.S.Audio.SfxVolumeScale != 1F)
        {
            GameManager.S.Audio.SfxVolumeScale = 1F;
        }


        // check for progress per frame, e.g. completed dialog
        if(this.currentLevel == Level.TutLookAt)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
            {
                if (hit.collider.gameObject.tag == "God" || hit.collider.gameObject.tag == "Grunt")
                {
                    Progress();
                }
            }
        }
    }

    public void GameStart()
    {
        Debug.Log("Game Start!");
        Progress(Level.Start + 1);
    }

    public void WaveStart(int wave)
    {
        Progress((Level) 6 + wave);
    }

    public void Progress()
    {
        Progress(currentLevel + 1);
    }

    /// <summary>
    /// Mark progress from a certain external action
    /// </summary>
    public void Progress(Level switchToLevel)
    {
        Debug.Log("Progressing to Level " + 1);
        currentLevel = switchToLevel;
        switch(currentLevel)
        {
            case Level.Start:
                // show start menu
                menuObject.SetActive(true);
                // disabled everything else if we forgot
                god.SetActive(false);
                tutorialFire.SetActive(false);
                tutorialIce.SetActive(false);
                tutorialGravity.SetActive(false);
                tutorialTargetFire.SetActive(false);
                tutorialTargetIce.SetActive(false);
                tutorialTargetGravity.SetActive(false);
                shield.SetActive(false);
                waveManager.SetActive(false);
                GameManager.S.Input.active = false;
                break;
            case Level.TutLookAt:
                // hide start menu
                god.SetActive(true);
                god.GetComponent<Collider>().enabled = true;
                menuObject.SetActive(false);
                // start god talking
                startAudioClips(this.lookAt);
                break;
            case Level.TutFire:
                // start next god talking
                god.GetComponent<Collider>().enabled = false;
                startAudioClips(this.tutFire);
                tutorialFire.SetActive(true);
                tutorialIce.SetActive(false);
                tutorialGravity.SetActive(false);
                tutorialTargetFire.SetActive(true);
                GameManager.S.Input.active = true;
                // start hand anim
                break;
            case Level.TutIce:
                // start next god talking
                // start hand anim
                startAudioClips(this.tutIce);
                tutorialFire.SetActive(false);
                tutorialIce.SetActive(true);
                tutorialGravity.SetActive(false);
                tutorialTargetFire.SetActive(false);
                tutorialTargetIce.SetActive(true);
                break;
            case Level.TutSpecial:
                // start next god talking
                // start hand anim
                startAudioClips(this.tutSpecial);
                tutorialFire.SetActive(false);
                tutorialIce.SetActive(false);
                tutorialGravity.SetActive(true);
                tutorialTargetIce.SetActive(false);
                tutorialTargetGravity.SetActive(true);
                break;
            case Level.TutAttackGod:
                startAudioClips(this.tutAttackGod);
                god.GetComponent<Collider>().enabled = true;
                tutorialGravity.SetActive(false);
                tutorialTargetGravity.SetActive(false);
                break;
            case Level.Level1:
                // start end god talking
                GameManager.S.Audio.PlayLevelStartBgm();
                god.SetActive(false);
                startAudioClips(this.tutEnd);
                shield.SetActive(true);
                waveManager.SetActive(true);
                // start wave after set time
                break;
            case Level.Level2:
                startAudioClips(this.level2);
                break;
            case Level.Level3:
                startAudioClips(this.level3);
                break;
            case Level.Level4:
                startAudioClips(this.level4);
                break;
            case Level.End:
                god.SetActive(true);
                god.GetComponent<Collider>().enabled = false;
                startAudioClips(this.end);
                ending.EndGame();
                break;

        }
    }

    public void ObeliskDown()
    {
        obelisksDown++;

        switch(obelisksDown)
        {
            case 1:
                startAudioClips(this.obelisk1down);
                break;
            case 2:
                startAudioClips(this.obelisk2down);
                break;
            default:
                break;
        }
    }

    public void GameOver()
    {
        this.startAudioClips(this.gameOver);
    }

    public void startAudioClips(List<AudioClip> clips)
    {
        this.audioSource.enabled = false;
        this.currentAudioClips = clips;
        this.currentClipIndex = -1;

        GameManager.S.Audio.SfxVolumeScale = 0.5F;
    }

}
