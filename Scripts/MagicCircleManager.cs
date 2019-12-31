using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCircleManager : MonoBehaviour
{
#pragma warning disable 0649
    [Tooltip("Array of circles, outermost first")]
    [SerializeField] private GameObject[] circles;

#pragma warning restore 0649

    private int currCircle = 0;

    public void DisableNextCircle()
    {
        if (currCircle == circles.Length) return;
        circles[currCircle].gameObject.SetActive(false);
        currCircle++;
    }

}
