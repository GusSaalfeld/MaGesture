using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameManager
{
    IInputManager Input { get; }
    IAudioManager Audio { get; }
}

public class GameManager : MonoBehaviour, IGameManager
{
    public static IGameManager S { get; private set; }

    #region IGameManager
    public IInputManager Input { get; private set; }
    public IAudioManager Audio { get; private set; }
    #endregion

    #region Inspector
#pragma warning disable 0649
    [SerializeField] private AudioSettings audioSettings;
    [SerializeField] private InputSettings inputSettings;
#pragma warning restore 0649
    #endregion

    #region Unity Events
    private void Awake()
    {
        if (S != null)
        {
            Debug.LogError("DESTROYING OLD GAMEMANAGER because old GameManager object was detected.");
            Destroy((GameManager)S);
            S = null;
        }

        //Check for old input managers
        InputManager[] oldInputManagers = FindObjectsOfType<InputManager>();
        if (oldInputManagers.Length > 0)
        {
            Debug.LogError("DESTROYING SELF.  Failed to initialize because old InputManager objects were detected.");
            Destroy(this);
        }

        //Check for old audio managers
        AudioManager[] oldAudioManagers = FindObjectsOfType<AudioManager>();
        if (oldAudioManagers.Length > 0)
        {
            Debug.LogError("DESTROYING SELF.  Failed to initialize because old AudioManager objects were detected.");
            Destroy(this);
        }

        S = this;

        //Initialize input manager
        InputManager input = gameObject.AddComponent<InputManager>();
        input.Initialize(inputSettings);
        Input = input;

        //Initialize audio manager
        AudioManager audio = gameObject.AddComponent<AudioManager>();
        audio.Initialize(audioSettings);
        Audio = audio;
    }

    public void OnDestroy()
    {
        Destroy((InputManager)Input);
        Destroy((AudioManager)Audio);
    }
    #endregion
}
