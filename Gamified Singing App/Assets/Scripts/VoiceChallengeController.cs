using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Audio;
using UnityEngine.Timeline;
using TMPro;

public class VoiceChallengeController : MonoBehaviour
{
    [SerializeField] private Exercise exercise;
    [SerializeField] private Transform spawnPosition, middlePosition, endPosition;
    private Glass currentGlass;
    private GameObject newGlass;
    public float movementDuration = 20.0f, score = 0;
    [SerializeField] GameObject prefabGlass;
    [SerializeField] RectTransform parentPanel;
    [SerializeField] TMP_Text textScore;
    [SerializeField] PlayerConfig playerConfig;

    #region Pitch Visualizer
    public AudioSource audioSource;
    public AudioPitchEstimator estimator;
    public LineRenderer lineFrequency;
    public TMP_Text textFrequency;
    #endregion

    private float frequency = 1;
    private Tween tween;

    // for testing purposes
    //public float newFrequency = 50;

    public int estimateRate = 30;

    public void Start()
    {
        // Initialize the current data & exercise
        LoadPlayerConfig();

        // Feed the 1st glass to the player so they can sing
        Invoke("NextGlass", 1.0f);

        // call at slow intervals (Update() is generally too fast)
        InvokeRepeating(nameof(UpdateVisualizer), 0, 1.0f / estimateRate);
    }

    public void FixedUpdate()
    {
        // Compute score & glass dmg as long as there's a glass + the user is singing

        if(currentGlass != null && lineFrequency.positionCount > 0)
        {
            ComputeGlassScore();
        }
    }

    public void LoadPlayerConfig()
    {
        // Load the user's frequency range and selected exercise
        estimator.frequencyMin = (int) playerConfig.minSingingFreq;
        estimator.frequencyMax = (int) playerConfig.maxSingingFreq;
        exercise = playerConfig.selectedExercise;
        exercise.Reset();
    }

    public void NextGlass()
    {
        currentGlass = exercise.NextGlass();

        // If no more Glasses, win!
        if (currentGlass == null)  // AND all present glasses are won
        {
            Debug.Log("End");
            // WIN ~ SceneManager.LoadScene(0);
            return;
        }

        Debug.Log(currentGlass.frequencyBreak);

        // Make glass appear 
        // TODO prettier animation
        
        if(newGlass == null)
        {
            newGlass = Instantiate(prefabGlass, parentPanel);
        }

        newGlass.GetComponent<Image>().sprite = currentGlass.sprite;

        // Move the glass
        newGlass.transform.position = spawnPosition.position;

        //newGlass.transform.DOMove(endPosition.position, movementDuration).SetEase(Ease.Linear).OnComplete(() => GlassEnd(newGlass)); // Go to end
        tween = newGlass.transform.DOMove(middlePosition.position, movementDuration / 5); // Go to middle

        // TODO end animation

        // scale glass according to pitch, Ideally scaling should be minimal, and sprite should be sized according to pitch.

        // 100
        // Range - 50 - 1000
        // Screen height 0 - 1080
        // Freq edges (1/6 & 5/6) - 180 - 720

        // TODO cull glasses that are outside of the player's vocal range

        var freqToHeight = Map(currentGlass.frequencyBreak, estimator.frequencyMin, estimator.frequencyMax, 0, 720);
        newGlass.GetComponent<RectTransform>().sizeDelta = new Vector2(freqToHeight, freqToHeight);

        // Y mil cosas mas que saldrán, pero eso pa luego

        newGlass.GetComponent<Button>().onClick.AddListener(() => GlassClicked(currentGlass.sound));
    }

    public void GlassEnd()
    {
        // We remove the current glass and feed the next one (if any)
        //GameObject.Destroy(newGlass);
        DOTween.Kill(tween);
        NextGlass();
    }

    public void GlassClicked(AudioClip glassSound)
    {
        Debug.Log("Ding!");
        //Play(glassSound);
    }

    public void ComputeGlassScore()
    {
        float voicePrecision = Mathf.Abs(currentGlass.frequencyBreak - frequency);

        // 1hz range = perfect score
        // 5hz range = medium score
        // 15hz range = min score
        // 30hz range = no score

        if (voicePrecision < 30)
        {
            currentGlass.toughness -= 0.02f;
            score += 1;

            if(voicePrecision < 15)
            {
                score += 2;
                if(voicePrecision < 5)
                {
                    score += 3;
                    if(voicePrecision <1)
                    {
                        score += 4;
                    }
                }
            }

            if (currentGlass.toughness <= 0)
            {
                score += 100; // 100 score per broken glass
                GlassEnd();
            }

            textScore.text = "Score\r\n" + score;
        }
    }

    void UpdateVisualizer()
    {
        // estimate the fundamental frequency
        var newFrequency = estimator.Estimate(audioSource);

        if (float.IsNaN(newFrequency))
        {
            // hide the line when it does not exist
            lineFrequency.positionCount = 0;
        }
        else
        {
            DOTween.To(() => frequency, x => frequency = x, newFrequency, 0.5f); // Smooth frequency visualization

            // indicate the frequency with LineRenderer
            var cam = Camera.main;
            var freqToHeight = Map(frequency, estimator.frequencyMin, estimator.frequencyMax, cam.scaledPixelHeight/6, cam.scaledPixelHeight-(cam.scaledPixelHeight/6));
            var worldStart = cam.ScreenToWorldPoint(new Vector3(0, freqToHeight, cam.nearClipPlane));
            var worldEnd = cam.ScreenToWorldPoint(new Vector3(cam.scaledPixelWidth, freqToHeight, cam.nearClipPlane)); 

            lineFrequency.positionCount = 2;
            lineFrequency.SetPosition(0, worldStart);
            lineFrequency.SetPosition(1, worldEnd); // TODO Lerp/DOTween the positions for smoother line

            // Display frequency and name of note
            textFrequency.text = "Frequency:\r\n" + frequency + " HZ\r\n" + GetNameFromFrequency(frequency);
        }
    }

    public static float Map(float value, float A, float B, float C, float D)
    {
        // Converts float value from range A-B to range C-D
        return C + (value - A) * (D - C) / (B - A);
    }

    string GetNameFromFrequency(float frequency)
    {
        if(!float.IsNaN(frequency))
        {
            // frequency -> pitch name
            var noteNumber = Mathf.RoundToInt(12 * Mathf.Log(frequency / 440) / Mathf.Log(2) + 69);
            string[] names = {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
            };
            return names[noteNumber % 12];
        }
        return "";
    }
}
