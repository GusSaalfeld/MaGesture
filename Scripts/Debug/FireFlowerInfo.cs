using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireFlowerInfo : SpellInfo
{
#pragma warning disable 0649
    [SerializeField] private FireFlower spell;
#pragma warning restore 0649

    protected override Spell Spell
    {
        get
        {
            if (spell == null) spell = FindObjectOfType<FireFlower>();
            return spell;
        }
    }

    protected override string GetInfoText()
    {
        List<string> sb = new List<string>();
        sb.Add(string.Format("Chain: {0}/200\n", spell.Chain.ToString()));
        sb.Add(string.Format("Cooldown: {0}/200\n", spell.Cooldown.ToString()));

        return string.Concat(sb);
    }
}
