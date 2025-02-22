using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioRecorder : MonoBehaviour
{
    public AudioSource audioSource;
    public int duration = 8;
    public AudioMixerGroup microphoneMixer;

    void Start()
    {
        audioSource.outputAudioMixerGroup = microphoneMixer; // Set to a different channel so no echo is heard
        audioSource.clip = Microphone.Start(string.Empty, audioSource.loop, duration, AudioSettings.outputSampleRate);
        audioSource.Play();
    }

}
