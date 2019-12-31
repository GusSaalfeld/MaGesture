using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceMistInfo : SpellInfo
{
#pragma warning disable 0649
    [SerializeField] private IceMist spell;
#pragma warning restore 0649

    protected override Spell Spell
    {
        get
        {
            if (spell == null) spell = FindObjectOfType<IceMist>();
            return spell;
        }
    }

    protected override string GetInfoText()
    {
        List<string> sb = new List<string>();
        sb.Add(string.Format("Chill: {0}/200\n", spell.Chill.ToString()));
        sb.Add(string.Format("Splash: {0}/200\n", spell.Splash.ToString()));
        sb.Add("\n");
        sb.Add(string.Format("Radius: {0}\n", spell.Radius.ToString()));

        return string.Concat(sb);
    }
}
