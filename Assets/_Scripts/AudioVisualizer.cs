using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioVisualizer : MonoBehaviour {

    [HideInInspector]
    public static AudioVisualizer instance = null;

    [Tooltip("Must be a power of 2, min 64, max 8192")]
    public int numSamples = 512;
    public FFTWindow spectrumAnalysisType = FFTWindow.Blackman;

    [Tooltip("Enter permalink to soundcloud address, currently disabled :(")]
    public string soundcloudLink = "https://api.soundcloud.com/tracks/193781466";

    //To be used to store frequency band information
    private float[] samples;
    private float[] bandRanges;
    private float[] bandValues;
    private float hzPerSample;

    //To be used for beat detection
    private float[] samplesLeft;
    private float[] samplesRight;
    private LinkedList<float> sampleHistory;

    private AudioSource audioSource;

    //Create a singleton that doesn't destroy itself inbetween scenes
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start () {
        audioSource = GetComponent<AudioSource>();

        //Create buffers
        samples = new float[numSamples];
        samplesLeft = new float[numSamples];
        samplesRight = new float[numSamples];
        bandValues = new float[8];
        bandRanges = new float[8];

        //Create linked list to store beat detection data, to be used as a moving average
        sampleHistory = new LinkedList<float>();
        for(int i = 0; i < (int)(1 / Time.fixedDeltaTime); i++)
        {
            sampleHistory.AddFirst(0);
        }

        
        //The hz per sample is the max frequency in the sample / sample length
        //http://answers.unity3d.com/questions/157940/getoutputdata-and-getspectrumdata-they-represent-t.html
        hzPerSample = (AudioSettings.outputSampleRate/2) / samples.Length;

        Debug.Log("Audio sample rate: " + AudioSettings.outputSampleRate);
        Debug.Log("Hz per sample: " + hzPerSample);
        
        makeFrequencyBands();

        /*
         * Waiting for soundcloud api access
         * 
        if(soundcloudLink != "")
        {
            StartCoroutine(playSoundcloud());
        }
        */
    }

    //Called 50 times/second if possible, based on physics update
    void FixedUpdate()
    {
        if (audioSource.isPlaying)
        {
            getSpectrumData();
            updateBandValues();
            //updateBeatData();
        }
    }

    /// <summary>
    /// Get sample data from current audio playing
    /// </summary>
    private void getSpectrumData()
    {
        audioSource.GetSpectrumData(samples, 0, spectrumAnalysisType);
        //audioSource.GetSpectrumData(samplesLeft, 0, spectrumAnalysisType);
        //audioSource.GetSpectrumData(samplesRight, 1, spectrumAnalysisType);
    }

    /// <summary>
    /// Update beat data history
    /// </summary>
    private void updateBeatData()
    {
        float instantEnergy = 0;
        for (int i = 0; i < samplesLeft.Length; i++)
        {
            instantEnergy += (samplesLeft[i] * samplesLeft[i]) + (samplesRight[i] * samplesRight[i]);
        }

        sampleHistory.RemoveLast();
        sampleHistory.AddFirst(instantEnergy);
    }

    /// <summary>
    /// Update the current band range values based on the average amplitude over the band ranges in the sample data
    /// </summary>
    private void updateBandValues()
    {
        int prevIndex = 0;
        for(int i = 0; i < bandRanges.Length; i++)
        {
            float currentTotalAmplitude = 0f;
            int j;
            for(j = prevIndex; j <= bandRanges[i]; j++ )
            {
                currentTotalAmplitude += samples[j];
            }
            bandValues[i] = currentTotalAmplitude / (j - prevIndex);
            prevIndex = j+1;
        }
    }

    /// <summary>
    /// Creates audio ranges for each frequency band range based on number of samples and hz per sample
    /// </summary>
    private void makeFrequencyBands()
    {
        /* Frequency ranges
         * Sub Bass: 20-60 hz
         * Bass: 60-250hz
         * Low Midrange: 250-500 hz
         * Midrange: 500-2k hz
         * Upper Midrange: 2k-4k hz
         * Presence: 4k-6k hz
         * Brilliance: 6k-20k hz
        */
         
        //Goal ranges for each band
        float[] ranges = new float[] {60,250,500,2000,4000,6000,10000,AudioSettings.outputSampleRate/2};

        float currentFrequency = 0f;
        int currentBandIndex = 0;

        //Sum the total frequency traversed and check if it has surpassed the goal range for the current band
        for(int i = 0; i < samples.Length; i++)
        {
            currentFrequency += hzPerSample;

            //Move to next band range after getting within half a step or going over 
            if((ranges[currentBandIndex] - currentFrequency) < hzPerSample / 2f || currentFrequency >= ranges[currentBandIndex])
            {
                bandRanges[currentBandIndex] = i;
                currentBandIndex++;
                if (currentBandIndex >= bandRanges.Length-1)
                    break;
            }
        }
        //Set the last range goal to be the max frequency value
        bandRanges[7] = samples.Length-1;
    }

    //From https://gamedev.stackexchange.com/questions/112699/play-soundcloud-in-unity-3d
    /// <summary>
    /// Experimental soundcloud playback, currently waiting for api access
    /// </summary>
    /// <returns></returns>
    IEnumerator playSoundcloud()
    {
        WWW www = new WWW(soundcloudLink);
        while (!www.isDone)
        {
            yield return 0;
        }
        audioSource.clip = NAudioPlayer.FromMp3Data(www.bytes);
        //audioSource.clip = www.GetAudioClip(false, true);
    }

    /// <summary>
    /// Beat detection algorithm from http://archive.gamedev.net/archive/reference/programming/features/beatdetection/
    /// NOTE: It appears this does not work very well in general, searching for alternatives
    /// </summary>
    /// <returns></returns>
    private bool checkBeat()
    {

        float averageEnergy = 0f;
        foreach (float energy in sampleHistory)
        {
            averageEnergy += energy * energy;
        }
        averageEnergy /= sampleHistory.Count;

        float variance = 0f;

        foreach (float energy in sampleHistory)
        {
            variance += (energy - averageEnergy) * (energy - averageEnergy);
        }
        variance /= sampleHistory.Count;
        Debug.Log("avg: " + averageEnergy + ", first: " + sampleHistory.First.Value + ", variance: " + variance);

        float beatConstant = (-0.0000015f * variance) + 1.5142857f;

        //If instant energy is greater than average energy there is a beat
        if (sampleHistory.First.Value > averageEnergy * beatConstant)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the average float value for samples over audio range "range"
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public static float getRawAudioRange(AudioRange range)
    {
        switch(range)
        {
            case AudioRange.SubBass:
                return instance.bandValues[0];
            case AudioRange.Bass:
                return instance.bandValues[1];
            case AudioRange.LowMid:
                return instance.bandValues[2];
            case AudioRange.Mid:
                return instance.bandValues[3];
            case AudioRange.UpperMid:
                return instance.bandValues[4];
            case AudioRange.Presence:
                return instance.bandValues[5];
            case AudioRange.Brilliance:
                return instance.bandValues[6];
            case AudioRange.SuperBrilliance:
                return instance.bandValues[7];
            /*
            case AudioRange.Beat:
                if (instance.checkBeat())
                    return 1;
                else
                    return 0;
            */
            default:
                return 0;
        }
    }

    /// <summary>
    /// Returns all current audio data
    /// </summary>
    /// <returns>Current sample data</returns>
    public static float[] getRawAudioData()
    {
        return instance.samples;
    }

    /// <summary>
    /// Checks if audio source attached to this object is playing
    /// </summary>
    /// <returns></returns>
    public static bool audioIsPlaying()
    {
        return instance.audioSource.isPlaying;
    }
}
