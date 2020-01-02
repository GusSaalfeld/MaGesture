using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;

public class ElementFollow : MonoBehaviour
{
    private Controller controller;
    private bool onLeft;

    [SerializeField] private float distAbovePalm = 0.0f;

    private Coroutine follow;

    public void Initialize(Controller c, bool onLeftHand)
    {
        controller = c;
        onLeft = onLeftHand;
        follow = StartCoroutine(FollowHand());
    }

    public void StopFollow()
    {
        StopCoroutine(follow);
    }

    private IEnumerator FollowHand()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            Hand h = null;

            foreach(Hand item in controller.Frame().Hands)
            {
                if (item.IsLeft == onLeft)
                {
                    h = item;
                    break;
                }
            }

            if (h != null) transform.localPosition = new Vector3(h.PalmPosition.x / 1000, h.PalmPosition.y / 1000 + distAbovePalm, h.PalmPosition.z / 1000);
        }
    }
}
