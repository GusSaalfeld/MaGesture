using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    private SteamVR_LoadLevel levelLoader;

#pragma warning disable 0649
    [SerializeField] private Renderer fade;
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private GameObject[] enteringObjects;
    [SerializeField] private GameObject[] exitingObjects;

#pragma warning restore 0649

    private bool isRunning = false;

    private void Awake()
    {
        levelLoader = GetComponent<SteamVR_LoadLevel>();

        if (levelLoader.levelName == "") levelLoader.levelName = SceneManager.GetActiveScene().name;
    }

    public void Restart()
    {
        levelLoader.Trigger();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
    }

    public void StartGameOver()
    {
        LevelManager.singleton.GameOver();

        if (isRunning) return;
        isRunning = true;
        fade.gameObject.SetActive(true);
        StartCoroutine(RunGameOver());
        GameManager.S.Input.active = false;
    }

    private IEnumerator RunGameOver()
    {
        yield return StartCoroutine(FadeIn());

        foreach(GameObject item in exitingObjects)
        {
            item.SetActive(false);
        }
        foreach(GameObject item in enteringObjects)
        {
            item.SetActive(true);
        }

        yield return StartCoroutine(FadeOut());
        yield return null;
    }

    private IEnumerator FadeIn()
    {
        float time = 0;
        Material mat = fade.material;
        Color col = mat.color;
        while(time < fadeTime)
        {
            yield return new WaitForSeconds(fadeTime / 50);
            Debug.Log(col.a);
            col.a += 1 / (fadeTime * 50);
            mat.SetColor("_Color", col);
            time += fadeTime / 50;
        }
        yield return null;
    }

    private IEnumerator FadeOut()
    {
        float time = 0;
        Material mat = fade.material;
        Color col = mat.color;
        while (time < fadeTime)
        {
            yield return new WaitForSeconds(fadeTime / 50);
            col.a -= 1 / (fadeTime * 50);
            mat.SetColor("_Color", col);
            time += fadeTime / 50;
        }
        yield return null;
    }
}
