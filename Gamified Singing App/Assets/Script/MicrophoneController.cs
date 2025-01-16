using UnityEngine;

public class MicrophoneController : MonoBehaviour
{
    void EstimatePitch()
    {
        var estimator = this.GetComponent<AudioPitchEstimator>();
        var audioSource = this.GetComponent<AudioSource>();

        // Estimates fundamental frequency from AudioSource output.
        float frequency = estimator.Estimate(audioSource);

        if (float.IsNaN(frequency))
        {
            // Algorithm didn't detect fundamental frequency (e.g. silence).
        }
        else
        {
            // Algorithm detected fundamental frequency.
            // The frequency is stored in the variable `frequency` (in Hz).
            Debug.Log("freq: " +frequency);
        }
    }

    void Update()
    {
        // It is NOT recommended to run the estimation every frame.
        // This will take a high computational load.
        // EstimatePitch();
    }

    void Start()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = Microphone.Start("", true, 1, 44100);
        audioSource.Play();

        // It is recommended to estimate at appropriate time intervals.
        // This example runs every 0.1 seconds.
        InvokeRepeating("EstimatePitch", 0, 0.1f);
    }
}

