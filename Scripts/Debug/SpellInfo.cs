using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class SpellInfo : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private GameObject attrAdjustBtns;
#pragma warning restore 0649

    protected abstract Spell Spell { get; }

    private Text text;
    private Button[] buttons;

    protected abstract string GetInfoText();

    private void OnEnable()
    {
        if (text == null)
        {
            text = GetComponentInChildren<Text>();
        }
        if (attrAdjustBtns != null)
        {
            InitializeButtons();
        }
        StopAllCoroutines();
        StartCoroutine(UpdateInfo());
    }

    private IEnumerator UpdateInfo()
    {
        while (true)
        {
            List<string> sb = new List<string>();
            if (Spell != null)
            {
                sb.Add(string.Format("__Aim__\n{0}\n\n", Spell.Aim.ToString()));
                sb.Add(GetInfoText());
            }
            else
            {
                sb.Add("No Spell is set.");
            }

            text.text = string.Concat(sb);
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    private void InitializeButtons()
    {
        if (attrAdjustBtns == null)
        {
            return;
        }
        buttons = attrAdjustBtns.GetComponentsInChildren<Button>();
        Button b;

        b = buttons[0];
        b.GetComponentInChildren<Text>().text = "Force+";
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => AdjustSpell(SpellAttribute.Force, 10));

        b = buttons[1];
        b.GetComponentInChildren<Text>().text = "Force-";
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => AdjustSpell(SpellAttribute.Force, -10));

        b = buttons[2];
        b.GetComponentInChildren<Text>().text = "Skill+";
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => AdjustSpell(SpellAttribute.Skill, -10));

        b = buttons[3];
        b.GetComponentInChildren<Text>().text = "Skill-";
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => AdjustSpell(SpellAttribute.Skill, 10));
    }

    private void AdjustSpell(SpellAttribute attr, int delta)
    {
        if (Spell == null)
        {
            return;
        }

        Spell.AdjustAttribute(attr, delta);
    }
}
