using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using System.IO;
using Leap.Unity;

public class GestureRecorder : MonoBehaviour
{
    private Controller controller;
    private Frame[] handSnapshots;

    private List<Vector3> recordedPositionsLeft;
    private List<Vector3> recordedPositionsRight;
    private Vector3 lastPosLeft;
    private Vector3 lastPosRight;

    private Coroutine motionRecording;

    public string filename = "";

    private void Awake()
    {
        controller = new Controller();

        string[] files = Directory.GetFiles(Application.dataPath + "/Gestures/Snapshots");
        handSnapshots = new Frame[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta")) continue;
            StreamReader reader = new StreamReader(files[i]);

            handSnapshots[i] = JsonUtility.FromJson<Frame>(reader.ReadToEnd());
            reader.Close();
        }
    }

    public void StoreHandSnapshot()
    {
        Debug.Log("Saving gesture...");

        StreamWriter writer = new StreamWriter(Application.dataPath + "/Gestures/Snapshots/" + filename + ".json", false);
        Frame frame = controller.Frame();

        writer.Write(JsonUtility.ToJson(frame));


        writer.Close();
    }

    public Frame[] GetHandSnapshots()
    {
        return handSnapshots;
    }

    public void RecordHandMotion()
    {
        Debug.Log("Recording motion...");
        List<Hand> hands = controller.Frame().Hands;
        Hand left = null;
        Hand right = null;
        foreach (Hand item in hands)
        {
            if (item.IsLeft) left = item;
            else right = item;
        }
        recordedPositionsLeft.Add(Vector3.zero);
        lastPosLeft = left.PalmPosition.ToVector3();
        //Right
        //Start Coroutine

    }

    private IEnumerator recordMotion()
    {
        List<Hand> hands = controller.Frame().Hands;
        Hand left = null;
        Hand right = null;
        foreach(Hand item in hands)
        {
            if (item.IsLeft) left = item;
            else right = item;
        }

        recordedPositionsLeft.Add(left.PalmPosition.ToVector3() - lastPosLeft);
        lastPosLeft = left.PalmPosition.ToVector3();

        //Right

        yield return new WaitForEndOfFrame();
    }
}
