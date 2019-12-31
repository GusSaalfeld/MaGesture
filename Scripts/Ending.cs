using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Ending : MonoBehaviour
{
    private ChromaticAberration glitch;
    private Material missingTex;

    private bool running = false;

    private Renderer[] glitchables;

#pragma warning disable 0649
    [SerializeField] private GameObject ring;
    [SerializeField] private GameObject meteors;
    [SerializeField] private GameObject environmentParent;
    [SerializeField] private bool startOnAwake;
#pragma warning restore 0649

    private void Awake()
    {
        PostProcessVolume vol = GetComponent<PostProcessVolume>();
        vol.profile.TryGetSettings(out glitch);
        missingTex = (Material) Resources.Load("Glitch", typeof(Material));
        glitchables = environmentParent.GetComponentsInChildren<Renderer>();
    }

    private void Start()
    {
        if (startOnAwake) EndGame();
    }

    public void EndGame()
    {
        if (running) return;
        StartCoroutine(TheEnd());
        GameManager.S.Input.active = false;

    }

    private IEnumerator TheEnd()
    {
        yield return new WaitForSeconds(5f);
        Instantiate(ring);
        yield return new WaitForSeconds(9f);
        Instantiate(meteors);
        GameManager.S.Audio.MeteorFall();
        yield return new WaitForSeconds(5f);
        StartCoroutine(removeTextures());
        yield return new WaitForSeconds(1f);
        StartCoroutine(chromAber());
        yield return new WaitForSeconds(8f);
        Time.timeScale = 0;
        GameManager.S.Audio.GameOver();
        yield return new WaitForSecondsRealtime(1f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
    }

    private IEnumerator removeTextures()
    {
        for(int i = 0; i < glitchables.Length / 2; i++)
        {
            int rng = Random.Range(0, glitchables.Length);
            if (glitchables[rng] == null)
            {
                i--;
                continue;
            }
            else
            {
                glitchables[rng].material = missingTex;
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            }
        }
    }

    private IEnumerator chromAber()
    {
        while (running)
        {
            glitch.intensity.value = Random.Range(0.2f, 1);
            yield return new WaitForSeconds(Random.Range(0.5f, 1));
        }
    }
}
