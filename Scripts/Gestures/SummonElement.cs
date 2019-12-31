using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;

public class SummonElement : MonoBehaviour
{
    Controller controller;

#pragma warning disable 0649

    //References to element prefabs
    [SerializeField] private GameObject elementBasic;
    [SerializeField] private GameObject elementBasicProjectile;

    //Gestures
    [SerializeField] private TextAsset summonGestureJSON;
    [SerializeField] private TextAsset shootGestureJSON;

    [SerializeField] private bool isLeft;

    [SerializeField] private Transform palmNormal;

#pragma warning restore 0649

    //Currently spawned element
    private GameObject currElement;

    private Frame summonGesture;
    private Frame shootGesture;

    private int gestureIndex = -1;
    private int gestureFrames = 0;

    private void Awake()
    {
        controller = new Controller();
    }

    private void Start()
    {
        //summonGesture = JsonUtility.FromJson<Frame>(summonGestureJSON.text);
        //shootGesture = JsonUtility.FromJson<Frame>(shootGestureJSON.text);
    }

    private void Update()
    {
        Frame frame = controller.Frame();
        List<Hand> hands = frame.Hands;

        foreach(Hand item in hands)
        {
            //if(item.IsLeft == isLeft)
                //CheckElementGesture(item);
        }
        
        
    }

    public void SpawnElement()
    {
        if (currElement != null) return;
        currElement = Instantiate(elementBasic, transform);
        currElement.transform.localPosition = new Vector3(0, -500f, 0);
    }

    public void ShootProjectile()
    {
        if (currElement == null) return;
        Quaternion adjustedNormal = Quaternion.LookRotation(palmNormal.forward + new Vector3(0, 0.3f, 0));
        Instantiate(elementBasicProjectile, transform.position, adjustedNormal);
        Destroy(currElement);
        currElement = null;
    }

    private void CheckElementGesture(Hand hand)
    {
        Debug.DrawRay(transform.position, palmNormal.forward + new Vector3(0,0.3f,0));
        if (currElement != null)
        {
            Hand targetHand = null;
            List<Hand> temp = shootGesture.Hands;
            foreach (Hand item in temp)
            {
                if (item.IsLeft == isLeft) targetHand = item;
            }
            if (targetHand == null)
            {
                Debug.LogError("Missing Gesture");
                return;
            }

            if (lenientAccuracy(hand.PalmNormal.Roll, targetHand.PalmNormal.Roll, 0.6f)
                && lenientAccuracy(hand.PalmNormal.Yaw, targetHand.PalmNormal.Yaw, 0.5f)
                && lenientAccuracy(hand.PalmNormal.Pitch, targetHand.PalmNormal.Pitch, 1.5f)
                && lenientAccuracy(hand.GrabAngle, targetHand.GrabAngle, 0.3f, false))
            {
                if (gestureIndex == 1 && gestureFrames < 5)
                {
                    gestureFrames++;
                }
                else if (gestureIndex == 1)
                {
                    Quaternion adjustedNormal = Quaternion.LookRotation(palmNormal.forward + new Vector3(0, 0.3f, 0));
                    Instantiate(elementBasicProjectile, transform.position, adjustedNormal);
                    Destroy(currElement);
                    currElement = null;

                    gestureIndex = -1;
                    gestureFrames = 0;
                }
                else
                {
                    gestureIndex = 1;
                    gestureFrames = 0;
                }
            }
            else if (hand.GrabAngle > 3f)
            {
                Destroy(currElement);
                currElement = null;
            }
        }
        else
        {
            Hand targetHand = null;
            List<Hand> temp = summonGesture.Hands;
            foreach (Hand item in temp)
            {
                if (item.IsLeft == isLeft) targetHand = item;
            }
            if (targetHand == null)
            {
                Debug.LogError("Missing Gesture");
                return;
            }

            if (lenientAccuracy(hand.PalmNormal.Roll, targetHand.PalmNormal.Roll, 0.5f)
                && lenientAccuracy(hand.PalmNormal.Yaw, targetHand.PalmNormal.Yaw, 0.5f)
                && lenientAccuracy(hand.PalmNormal.Pitch, targetHand.PalmNormal.Pitch, 0.5f)
                && lenientAccuracy(hand.GrabAngle, targetHand.GrabAngle, 0.3f, false))
            {
                if (gestureIndex == 0 && gestureFrames < 5)
                {
                    gestureFrames++;
                }
                else if (gestureIndex == 0)
                {
                    currElement = Instantiate(elementBasic, transform);
                    currElement.transform.localPosition = new Vector3(0, -500f, 0);

                    gestureIndex = -1;
                    gestureFrames = 0;
                }
                else
                {
                    gestureIndex = 0;
                    gestureFrames = 0;
                }


            }
        }

        
    }

    private float LoopPi(float input)
    {
        return Mathf.Repeat(input + Mathf.PI, Mathf.PI * 2) - Mathf.PI;
    }

    private bool lenientAccuracy(float curr, float target, float leniency, bool loopPi = true)
    {
        if(loopPi)
            return (curr > LoopPi(target - leniency) || curr > Mathf.Max(target - leniency, -Mathf.PI)) 
                    && (curr < LoopPi(target + leniency) || curr < Mathf.Min(target + leniency, Mathf.PI));
        else
            return (curr > target - leniency)
                    && (curr < target + leniency);
    }
}
