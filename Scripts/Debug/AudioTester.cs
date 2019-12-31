using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AudioTester : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button baseButton;
#pragma warning restore 0649

    private void Start()
    {
        IDictionary<UnityAction, string> buttonInfo = new Dictionary<UnityAction, string>()
        {
            { () => GameManager.S.Audio.SetBgmPlaying(true), "BGM On"},
            { () => GameManager.S.Audio.SetBgmPlaying(false), "BGM Off"},
            { GameManager.S.Audio.RestartBgm, "BGM Restart"},
            { GameManager.S.Audio.GameOver, "GameOver"},
            { () => GameManager.S.Audio.CastFire(), "Cast Fire"},
            { () => GameManager.S.Audio.FireHits(), "Fire Hit"},
            { () => GameManager.S.Audio.FireCharged(), "Fire Charged"},
            { () => GameManager.S.Audio.LoopFire(), "Looping Fire"},
            { () => GameManager.S.Audio.DismissFire(), "Dismiss Fire"},
            { () => GameManager.S.Audio.CastIce(), "Cast Ice"},
            { () => GameManager.S.Audio.IceHits(), "Ice Hit"},
            { () => GameManager.S.Audio.IceCharged(), "Ice Charged"},
            { () => GameManager.S.Audio.LoopIce(), "Looping Ice"},
            { () => GameManager.S.Audio.DismissIce(), "Dismiss Ice"},
            { () => GameManager.S.Audio.DismissSpell(), "Dismiss Spell"},
        };

        foreach (UnityAction buttonEffect in buttonInfo.Keys)
        {
            Button button = Instantiate(baseButton, transform);
            button.onClick.AddListener(buttonEffect);
            button.GetComponentInChildren<Text>().text = buttonInfo[buttonEffect];
            button.gameObject.SetActive(true);
        }
    }
}
