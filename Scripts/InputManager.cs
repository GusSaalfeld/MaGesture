using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using System.IO;
using Leap.Unity;

public interface IInputManager
{
    bool active { get; set; }
}

[System.Serializable]
public class InputSettings
{
    public string gestureFile;
    public SpellCaster spellCasterPrefab;

    //References for gesture information
    public Transform leftPalmNormal;
    public Transform rightPalmNormal;

    public Transform leftPalmAttach;
    public Transform rightPalmAttach;

    public bool useGazeAim;

    public bool useFingertipMode = false;
    public ThumbElementSelector leftThumb;
    public ThumbElementSelector rightThumb;

    [Header("Input Settings")]
    [Tooltip("Number of frames a gesture must be recognized before it is sent to spellCaster")]
    public int inputFrames;

    public void Validate()
    {
        if (gestureFile == "")
        {
            Debug.LogWarning("Gesture file not specified.  Please set a file " +
                "in the inspector (GameManager -> Input Settings)");
        }

        if (spellCasterPrefab == null)
        {
            Debug.LogWarning("SpellCaster prefab is not specified.  Please " +
                "set a reference in the inspector (GameManager -> Input Settings)");
        }

        if (leftPalmNormal == null || rightPalmNormal == null)
        {
            Debug.LogWarning("One or more attachment hand palm normals are not specified." +
                "Please set a reference in the inspector (GameManager -> Input Settings)");
        }

        if (useFingertipMode && (leftThumb == null || rightThumb == null))
        {
            Debug.LogWarning("Fingertip Mode is active, but left/right thumbs are not set.");
        }

        if (leftPalmAttach == null || rightPalmAttach == null)
        {
            Debug.LogWarning("One or more attachment hand palm references are missing from InputManager");
        }
    }
}

public class InputManager : MonoBehaviour, IInputManager
{
    private InputSettings settings;
    private ISpellCaster spellCasterRight;
    private ISpellCaster spellCasterLeft;
    private GestureRecorder gestureRecorder;
    private OSCReceiver oscReceiverRight;
    private OSCReceiver oscReceiverLeft;

    private Controller controller; //Leap Motion controller
    private GestureData[] gestures; //All saved 
    private Dictionary<string, GestureData> gestureDict = new Dictionary<string, GestureData>(); //Dictionary of GestureData with Wekinator addresses as the key

    private int framesTracked = 0; //Track number of frames a gesture has been recognized
    private string gestureTracked = ""; //Currently recognized gesture

    private bool waitLeft = false;
    private bool waitRight = false;

    public bool active { get; set; }

    public void Initialize(InputSettings inputSettings)
    {
        settings = inputSettings;
        settings.Validate();
    }

    #region Unity Events
    private void Awake()
    {
        controller = new Controller();
        active = true;
    }

    private void Start()
    {
        if (settings == null)
        {
            Debug.LogError("DESTROYING SELF.  InputSettings were not initialized.");
            Destroy(this);
            return;
        }

        spellCasterRight = Instantiate(settings.spellCasterPrefab, transform);
        spellCasterLeft = Instantiate(settings.spellCasterPrefab, transform);

        gestureRecorder = gameObject.AddComponent<GestureRecorder>();
        gestureRecorder.filename = settings.gestureFile;

        gestures = Resources.LoadAll<GestureData>("");

        OSCReceiver[] temp = GetComponentsInChildren<OSCReceiver>();
        if (temp.Length != 2)
        {
            Debug.LogError("Expected to find exactly two OSCReceivers.");
        }
        else if (temp[0] != null && !temp[0].Equals(null) && temp[1] != null && !temp[1].Equals(null))
        {
            if (temp[0].IsLeft)
            {
                oscReceiverLeft = temp[0];
                oscReceiverRight = temp[1];
            }
            else
            {
                oscReceiverRight = temp[0];
                oscReceiverLeft = temp[1];
            }

            foreach (GestureData item in gestures)
            {
                if (item.wekinatorAddress == "" || item.wekinatorAddress == null)
                {
                    continue;
                }
                gestureDict.Add(item.wekinatorAddress, item);

                if (item.wekinatorAddress.ToLower().Contains("left"))
                {
                    oscReceiverLeft.AddAddress(item.wekinatorAddress);
                }
                else if (item.wekinatorAddress.ToLower().Contains("right"))
                {
                    oscReceiverRight.AddAddress(item.wekinatorAddress);
                }
            }
        }

        if (settings.useFingertipMode)
        {
            if (settings.leftThumb)
            {
                settings.leftThumb.spellCaster = this.spellCasterLeft;
            }

            if (settings.rightThumb)
            {
                settings.rightThumb.spellCaster = this.spellCasterRight;
            }
        }
    }

    private void Update()
    {
        //Main gesture detection code
        //CheckForGesture();

        //Send Leap Data to Wekinator
        SendLeapData();

        //Quit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
        }
    }
    #endregion

    #region Gesture Detection
    //Manual detection
    private bool CheckHandMatch(Hand hand, Hand targetHand, GestureData gesture)
    {
        List<Finger> fingers = hand.Fingers;
        List<Finger> targetFingers = targetHand.Fingers;

        Quaternion palmRotation = Quaternion.Inverse(Quaternion.LookRotation(hand.PalmNormal.ToVector3()));
        Quaternion targetPalmRotation = Quaternion.Inverse(Quaternion.LookRotation(targetHand.PalmNormal.ToVector3()));

        //Grab each fingertip position relative to palm position
        Vector3 thumb = RotatePointAroundPivot(fingers[0].Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3(), hand.PalmPosition.ToVector3(), palmRotation) - hand.PalmPosition.ToVector3();
        Vector3 index = RotatePointAroundPivot(fingers[1].Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3(), hand.PalmPosition.ToVector3(), palmRotation) - hand.PalmPosition.ToVector3();
        Vector3 middle = RotatePointAroundPivot(fingers[2].Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3(), hand.PalmPosition.ToVector3(), palmRotation) - hand.PalmPosition.ToVector3();
        Vector3 ring = RotatePointAroundPivot(fingers[3].Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3(), hand.PalmPosition.ToVector3(), palmRotation) - hand.PalmPosition.ToVector3();
        Vector3 pinky = RotatePointAroundPivot(fingers[4].Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3(), hand.PalmPosition.ToVector3(), palmRotation) - hand.PalmPosition.ToVector3();

        Vector3 targetThumb = RotatePointAroundPivot(targetFingers[0].Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3(), targetHand.PalmPosition.ToVector3(), targetPalmRotation) - targetHand.PalmPosition.ToVector3();
        Vector3 targetIndex = RotatePointAroundPivot(targetFingers[1].Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3(), targetHand.PalmPosition.ToVector3(), targetPalmRotation) - targetHand.PalmPosition.ToVector3();
        Vector3 targetMiddle = RotatePointAroundPivot(targetFingers[2].Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3(), targetHand.PalmPosition.ToVector3(), targetPalmRotation) - targetHand.PalmPosition.ToVector3();
        Vector3 targetRing = RotatePointAroundPivot(targetFingers[3].Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3(), targetHand.PalmPosition.ToVector3(), targetPalmRotation) - targetHand.PalmPosition.ToVector3();
        Vector3 targetPinky = RotatePointAroundPivot(targetFingers[4].Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3(), targetHand.PalmPosition.ToVector3(), targetPalmRotation) - targetHand.PalmPosition.ToVector3();

        return LenientAccuracy(hand.PalmNormal.Roll, targetHand.PalmNormal.Roll, gesture.palmRollLeniency)
            && LenientAccuracy(hand.PalmNormal.Yaw, targetHand.PalmNormal.Yaw, gesture.palmYawLeniency)
            && LenientAccuracy(hand.PalmNormal.Pitch, targetHand.PalmNormal.Pitch, gesture.palmPitchLeniency)
            && LenientAccuracy(hand.GrabAngle, targetHand.GrabAngle, gesture.grabAngleLeniency, false)
            && LenientVector3(thumb, targetThumb, gesture.thumbLeniency)
            && LenientVector3(index, targetIndex, gesture.indexLeniency)
            && LenientVector3(middle, targetMiddle, gesture.middleLeniency)
            && LenientVector3(ring, targetRing, gesture.ringLeniency)
            && LenientVector3(pinky, targetPinky, gesture.pinkyLeniency);
    }

    private void CheckForGesture()
    {
        Frame frame = controller.Frame();
        Hand left = null;
        Hand right = null;
        foreach (Hand h in frame.Hands)
        {
            if (h.IsLeft) left = h;
            else right = h;
        }

        foreach (GestureData gestData in gestures)
        {
            if (gestData.gestureSnapshot == null) continue;
            Frame targetFrame = gestData.gestureSnapshot;
            Hand targetLeft = null;
            Hand targetRight = null;
            foreach (Hand h in targetFrame.Hands)
            {
                if (h.IsLeft) targetLeft = h;
                else targetRight = h;
            }

            if (gestData.twoHanded)
            {
                //TODO
            }
            else
            {
                //Left
                if (left != null && CheckHandMatch(left, targetLeft, gestData))
                {
                    if (gestureTracked == gestData.name)
                    {
                        if (framesTracked < settings.inputFrames)
                        {
                            framesTracked++;
                        }
                        else
                        {
                            gestureTracked = "";
                            framesTracked = 0;
                            Gesture g = new Gesture(gestData.type, settings.leftPalmAttach);
                            g.Aim = new Ray(settings.leftPalmAttach.position, settings.leftPalmNormal.forward + new Vector3(0, 0.2f, 0));
                            spellCasterLeft.PublishGesture(g);
                        }
                    }
                    else
                    {
                        gestureTracked = gestData.name;
                        framesTracked = 0;
                    }
                }
                else if (left != null)
                {
                    Gesture g = new Gesture(GestureType.None, settings.leftPalmAttach);
                    g.Aim = new Ray(settings.leftPalmAttach.position, settings.leftPalmNormal.forward + new Vector3(0, 0.2f, 0));
                    spellCasterLeft.PublishGesture(g);
                }
                //Right
                if (right != null && CheckHandMatch(right, targetRight, gestData))
                {
                    if (gestureTracked == gestData.name)
                    {
                        if (framesTracked < settings.inputFrames)
                        {
                            framesTracked++;
                        }
                        else
                        {
                            gestureTracked = "";
                            framesTracked = 0;
                            Gesture g = new Gesture(gestData.type, settings.rightPalmAttach);
                            g.Aim = new Ray(settings.rightPalmAttach.position, settings.rightPalmNormal.forward + new Vector3(0, 0.2f, 0));
                            spellCasterRight.PublishGesture(g);
                        }
                    }
                    else
                    {
                        gestureTracked = gestData.name;
                        framesTracked = 0;
                    }

                }
                else if (right != null)
                {
                    Gesture g = new Gesture(GestureType.None, settings.rightPalmAttach);
                    g.Aim = new Ray(settings.rightPalmAttach.position, settings.rightPalmNormal.forward + new Vector3(0, 0.2f, 0));
                    spellCasterRight.PublishGesture(g);
                }
            }
        }
    }

    //ML based detection (using Wekinator)
    public void ReceiveOutput(string address)
    {
        if (!active) return;
        GestureData data = gestureDict[address];

        if (data == null)
        {
            Debug.LogError("Wekinator Address received not in Gesture Dictionary");
            return;
        }

        if (address.ToLower().Contains("left"))
        {
            if (!waitLeft)
            {
                waitLeft = true;
                StartCoroutine(LimitInputLeft());

                Gesture g = new Gesture(data.type, settings.leftPalmAttach);
                g.Aim = new Ray(settings.leftPalmAttach.position, settings.leftPalmNormal.forward + new Vector3(0, 0.2f, 0));
                if (data.fingerAim)
                {
                    Frame frame = controller.Frame();
                    Hand hand = null;
                    foreach (Hand h in frame.Hands)
                    {
                        if (h.IsLeft) hand = h;
                    }
                    if (hand == null) Debug.LogError("You did a gesture with the left hand, but the left hand also doesn't exist????");
                    Finger pointer = hand.Fingers[1];
                    g.Aim = new Ray(pointer.TipPosition.ToVector3(), hand.Direction.ToVector3());
                }

                if (settings.useGazeAim)
                {
                    RaycastHit ray;
                    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out ray, 200, LayerMask.GetMask("Enemy", "Environment")))
                    {
                        g.Aim = new Ray(g.Aim.origin, ray.point - settings.rightPalmAttach.position);
                    }
                }

                spellCasterLeft.PublishGesture(g);
            }
        }
        if (address.ToLower().Contains("right"))
        {
            if (!waitRight)
            {
                waitRight = true;
                StartCoroutine(LimitInputRight());

                Gesture g = new Gesture(data.type, settings.rightPalmAttach);
                g.Aim = new Ray(settings.rightPalmAttach.position, settings.rightPalmNormal.forward + new Vector3(0, 0.2f, 0));
                if (data.fingerAim)
                {
                    Frame frame = controller.Frame();
                    Hand hand = null;
                    foreach (Hand h in frame.Hands)
                    {
                        if (!h.IsLeft) hand = h;
                    }
                    if (hand == null) Debug.LogError("You did a gesture with the right hand, but the right hand also doesn't exist????");
                    Finger pointer = hand.Fingers[1];
                    g.Aim = new Ray(pointer.TipPosition.ToVector3(), hand.Direction.ToVector3());
                }

                if (settings.useGazeAim)
                {
                    RaycastHit ray;
                    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out ray, 200, LayerMask.GetMask("Enemy", "Environment")))
                    {
                        g.Aim = new Ray(g.Aim.origin, ray.point - settings.rightPalmAttach.position);
                    }
                }

                spellCasterRight.PublishGesture(g);
            }
        }
    }

    private void SendLeapData()
    {
        ArrayList output = new ArrayList();
        Frame frame = controller.Frame();
        Hand left = null;
        Hand right = null;
        foreach (Hand h in frame.Hands)
        {
            if (h.IsLeft) left = h;
            else right = h;
        }

        //Left
        if (left == null)
        {
            for (int i = 0; i < 12; i++)
            {
                output.Add(0);
            }
        }
        else
        {
            //Palm data
            output.Add(left.PalmNormal.Roll * 100);
            output.Add(left.PalmNormal.Pitch * 100);
            output.Add(left.PalmNormal.Yaw * 100);
            output.Add(left.GrabAngle * 100);
            output.Add(left.PalmVelocity.x);
            output.Add(left.PalmVelocity.y);
            output.Add(left.PalmVelocity.z);

            //Fingers
            List<Finger> fingers = left.Fingers;
            for (int i = 0; i < 5; i++)
            {
                float fingerAngle = Vector3.Angle(left.Direction.ToVector3(), fingers[i].Direction.ToVector3());
                output.Add(fingerAngle * 2);
            }
        }

        if (oscReceiverLeft != null && left != null)
        {
            oscReceiverLeft.Send(output);
        }

        output = new ArrayList();

        //Right
        if (right == null)
        {
            for (int i = 0; i < 12; i++)
            {
                output.Add(0);
            }
        }
        else
        {
            //Palm data
            output.Add(right.PalmNormal.Roll * 100);
            output.Add(right.PalmNormal.Pitch * 100);
            output.Add(right.PalmNormal.Yaw * 100);
            output.Add(right.GrabAngle * 100);
            output.Add(right.PalmVelocity.x);
            output.Add(right.PalmVelocity.y);
            output.Add(right.PalmVelocity.z);

            //Fingers
            List<Finger> fingers = right.Fingers;
            for (int i = 0; i < 5; i++)
            {
                float fingerAngle = Vector3.Angle(right.Direction.ToVector3(), fingers[i].Direction.ToVector3());
                output.Add(fingerAngle * 2);
            }
        }

        if (oscReceiverRight != null && right != null)
        {
            oscReceiverRight.Send(output);
        }

    }


    #endregion

    #region Helper Functions
    private IEnumerator LimitInputLeft()
    {
        yield return new WaitForSeconds(0.25f);
        waitLeft = false;
    }

    private IEnumerator LimitInputRight()
    {
        yield return new WaitForSeconds(0.25f);
        waitRight = false;
    }

    private float LoopPi(float input)
    {
        return Mathf.Repeat(input + Mathf.PI, Mathf.PI * 2) - Mathf.PI;
    }

    private bool LenientAccuracy(float curr, float target, float leniency, bool loopPi = true)
    {
        if (loopPi)
            return (curr > LoopPi(target - leniency) || curr > Mathf.Max(target - leniency, -Mathf.PI))
                    && (curr < LoopPi(target + leniency) || curr < Mathf.Min(target + leniency, Mathf.PI));
        else
            return (curr > target - leniency)
                    && (curr < target + leniency);
    }

    private bool LenientVector3(Vector3 curr, Vector3 target, Vector3 leniency)
    {
        return LenientAccuracy(curr.x, target.x, leniency.x, false)
            && LenientAccuracy(curr.y, target.y, leniency.y, false)
            && LenientAccuracy(curr.z, target.z, leniency.z, false);
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        Vector3 dir = point - pivot;
        dir = rotation * dir;
        return dir + pivot;
    }

    #endregion
}
