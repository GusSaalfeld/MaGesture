using UnityEngine;

public class AudioGlow : MonoBehaviour
{
    public AudioSource audioSource;
    public float updateStep = 0.1f;
    public int sampleDataLength = 1024;

    public float maxLoudness = 0;
    public float minLoudness = 0;

    public float maxRange = 0;
    public float minRange = 0;

    public float maxGlow = 0;
    public float minGlow = 0;

    public Light targetLight;

    private float currentUpdateTime = 0f;

    private float clipLoudness;
    private float[] clipSampleData;

    // Use this for initialization
    void Awake()
    {

        if (!audioSource)
        {
            Debug.LogError(GetType() + ".Awake: there was no audioSource set.");
        }
        clipSampleData = new float[sampleDataLength];

    }

    // Update is called once per frame
    void Update()
    {

        currentUpdateTime += Time.deltaTime;
        if (currentUpdateTime >= updateStep)
        {
            currentUpdateTime = 0f;
            if (audioSource.clip != null)
            {
                audioSource.clip.GetData(clipSampleData, audioSource.timeSamples); //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
                clipLoudness = 0f;
                foreach (var sample in clipSampleData)
                {
                    clipLoudness += Mathf.Abs(sample);
                }
                clipLoudness /= sampleDataLength; //clipLoudness is what you are looking for

                //Debug.Log("cliploudness: " +clipLoudness);

                if (clipLoudness > maxLoudness)
                {
                    clipLoudness = maxLoudness;
                }

                if (clipLoudness < minLoudness)
                {
                    clipLoudness = minLoudness;
                }

                float glow = (clipLoudness - minLoudness) * (maxGlow - minGlow) / (maxLoudness - minLoudness) + minGlow;
                float range = (clipLoudness - minLoudness) * (maxRange - minRange) / (maxLoudness - minLoudness) + minRange;
                targetLight.intensity = glow;
                targetLight.range = range; 
            }
        }

    }

}