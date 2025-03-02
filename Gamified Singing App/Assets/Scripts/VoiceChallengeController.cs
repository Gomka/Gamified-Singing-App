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
    public float movementDuration = 20.0f, score = 0;
    [SerializeField] GameObject prefabGlass;
    [SerializeField] RectTransform parentPanel;
    [SerializeField] TMP_Text textScore;

    #region Pitch Visualizer
    public AudioSource audioSource;
    public AudioPitchEstimator estimator;
    public LineRenderer lineFrequency;
    public TMP_Text textFrequency;
    #endregion

    private float frequency = 1;

    // for testing purposes
    public float newFrequency = 50;

    public int estimateRate = 30;

    public void Start()
    {
        // Start the scene
        // Initialize the current exercise (quizas diferentes execises viven en diferentes scenes, con su arte y mood)

        // Feed the 1st glass to the player so they can sing
        exercise.Reset();
        Invoke("NextGlass", 1.0f);

        // call at slow intervals (Update() is generally too fast)
        InvokeRepeating(nameof(UpdateVisualizer), 0, 1.0f / estimateRate);
    }

    public void FixedUpdate()
    {
        // Compute score & glass dmg

        // 1hz range = perfect score
        // 5hz range = very high score
        // 15hz range = high score
        // 30hz range = medium score
        // 50hz range = low score
        
        if(currentGlass != null)
        {
            ComputeGlassScore();
        }
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


        GameObject newGlass = Instantiate(prefabGlass, parentPanel);
        newGlass.GetComponent<Image>().sprite = currentGlass.sprite;

        // Move the glass
        newGlass.transform.position = spawnPosition.position;
        
        //newGlass.transform.DOMove(endPosition.position, movementDuration).SetEase(Ease.Linear).OnComplete(() => GlassEnd(newGlass)); // Go to end
        newGlass.transform.DOMove(middlePosition.position, movementDuration / 5); // Go to middle

        // TODO end animation
        // TODO scale glass according to pitch, Ideally scaling should be minimal, and sprite should be sized according to pitch.

        // 100
        // Range - 50 - 1000
        // Screen height 0 - 1080
        // Freq edges (1/6 & 5/6) - 180 - 720

        var freqToHeight = Map(currentGlass.frequencyBreak, estimator.frequencyMin, estimator.frequencyMax, 0, 720);
        newGlass.GetComponent<RectTransform>().sizeDelta = new Vector2(freqToHeight, freqToHeight);
        // TODO BIG ONE: make pitch interact with glass
        // Y mil cosas mas que saldr�n, pero eso pa luego

        newGlass.GetComponent<Button>().onClick.AddListener(() => GlassClicked(currentGlass.sound));
    }

    public void GlassEnd(GameObject go)
    {
        GameObject.Destroy(go);
        NextGlass();
    }

    public void GlassClicked(AudioClip glassSound)
    {
        Debug.Log("Ding!");
        //Play(glassSound);
    }

    public void ComputeGlassScore()
    {
        float voicePrecision = currentGlass.frequencyBreak - frequency;
        //voicePrecision = 1/voicePrecision;

        Debug.Log(voicePrecision);

        score += voicePrecision;
        textScore.text = "Score\r\n" + score;

        /*
        switch (voicePrecision)
        {
            case < 2:
                score += 15;
                currentGlass.toughness -= 0.02f;
                break;
            case < 6:
                score += 8;
                currentGlass.toughness -= 0.02f;
                break;
            case < 16:
                score += 4;
                currentGlass.toughness -= 0.02f;
                break;
            case < 31:
                score += 2;
                currentGlass.toughness -= 0.02f;
                break;
            case < 51:
                score += 1;
                currentGlass.toughness -= 0.02f;
                break;
            default: break;
        }
        */
    }

    void UpdateVisualizer()
    {
        // estimate the fundamental frequency
        // var newFrequency = estimator.Estimate(audioSource);

        if (float.IsNaN(newFrequency))
        {
            // hide the line when it does not exist
            lineFrequency.positionCount = 0;
        }
        else
        {
            DOTween.To(() => frequency, x => frequency = x, newFrequency, 0.5f); // Smooth frequency change

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
        // frequency -> pitch name
        var noteNumber = Mathf.RoundToInt(12 * Mathf.Log(frequency / 440) / Mathf.Log(2) + 69);
        string[] names = {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
        };
        return names[noteNumber % 12];
    }
}
