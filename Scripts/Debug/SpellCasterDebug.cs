using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellCasterDebug: MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private SpellCaster spellCaster;

    [Header("Gestures")]
    [SerializeField] private RectTransform gestureButtons;
    [SerializeField] private Button baseButton;
#pragma warning restore 0649

    private Text text;

    private void Awake()
    {
        if (gestureButtons != null && baseButton != null)
        {
            AddGestureButtons();
        }
        else
        {
            Debug.LogError("SpellCasterDebug failed to initialize Gesture buttons.");
        }
    }

    private void OnEnable()
    {
        if (text == null)
        {
            text = GetComponentInChildren<Text>();
        }
        StopAllCoroutines();
        StartCoroutine(UpdateInfo());
    }

    private IEnumerator UpdateInfo()
    {
        while (true)
        {
            if (spellCaster == null)
            {
                spellCaster = FindObjectOfType<SpellCaster>();
            }

            List<string> sb = new List<string>();
            if (spellCaster == null)
            {
                sb.Add("No SpellCaster set.");
            }
            else
            {
                sb.Add(string.Format("Spell Caster State: {0}\n", spellCaster.State.ToString()));

                if (spellCaster.State == SpellCastingState.Select)
                {
                    sb.Add(string.Format("Element: {0}\n", spellCaster.SpellElement.ToString()));
                    sb.Add(string.Format("Tier: {0}\n", spellCaster.SpellTier.ToString()));
                }
                else if (spellCaster.State == SpellCastingState.Adjust)
                {
                    sb.Add(string.Format("Spell: {0}\n", spellCaster.CurrentSpell.Name));
                    sb.Add(string.Format("Element: {0}\n", spellCaster.CurrentSpell.Element.ToString()));
                    sb.Add(string.Format("Tier: {0}\n", spellCaster.CurrentSpell.Tier.ToString()));
                }
            }

            text.text = string.Concat(sb);
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    private void AddGestureButtons()
    {
        baseButton.gameObject.SetActive(false);
        foreach (GestureType g in System.Enum.GetValues(typeof(GestureType)))
        {
            string gName = System.Enum.GetName(typeof(GestureType), g);
            bool isDebug = gName.StartsWith("Debug");
            if (!isDebug) continue;

            gName = gName.Substring(5);

            Button newButton = Instantiate(baseButton);
            newButton.gameObject.SetActive(true);
            newButton.transform.SetParent(gestureButtons, false);
            newButton.GetComponentInChildren<Text>().text = gName + " (debug)";
            newButton.onClick.AddListener(() => PublishGesture(g));
        }
    }

    private void PublishGesture(GestureType gType)
    {
        if (spellCaster == null)
        {
            Debug.LogWarning("Cannot publish gesture. SpellCaster is null.");
            return;
        }

        Gesture g = new Gesture(gType, null);
        spellCaster.PublishGesture(g);
    }
}
