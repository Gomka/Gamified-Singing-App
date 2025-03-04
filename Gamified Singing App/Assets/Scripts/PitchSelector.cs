using UnityEngine;
using TMPro;

public class PitchSelector : MonoBehaviour
{
    public PlayerConfig playerConfig;
    public float minFreq = 300, maxFreq = 400, estimateRate = 30;
    public AudioSource audioSource;
    public AudioPitchEstimator estimator;
    [SerializeField] TMP_Text textMinFreq, textMaxFreq;

    public void Start()
    {
        // Begin detecting the highest and lowest frequency for the user
        // Update the PlayerConfig accordingly
        
        // call at slow intervals (Update() is generally too fast)
        InvokeRepeating(nameof(DetectMinMaxFreq), 0, 1.0f / estimateRate);
    }

    void DetectMinMaxFreq()
    {
        var frequency = estimator.Estimate(audioSource);

        if (!float.IsNaN(frequency))
        {
            if(frequency < minFreq)
            {
                minFreq = frequency;
                textMinFreq.text = "Min Freq.\r\n" + minFreq + "HZ";
                playerConfig.minSingingFreq = minFreq;

            } else if(frequency > maxFreq)
            {
                maxFreq = frequency;
                textMaxFreq.text = "Max Freq.\r\n" + maxFreq + "HZ";
                playerConfig.maxSingingFreq = maxFreq;
            }
        }
    }

    public void Reset()
    {
        minFreq = 300;
        playerConfig.minSingingFreq = minFreq;
        textMinFreq.text = "Min Freq.\r\n" + minFreq + "HZ";
        maxFreq = 400;
        playerConfig.maxSingingFreq = maxFreq;
        textMaxFreq.text = "Max Freq.\r\n" + maxFreq + "HZ";
    }
}
