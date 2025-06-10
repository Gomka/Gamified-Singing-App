using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Audio;
using UnityEngine.Timeline;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class ArcadeChallengeController : MonoBehaviour
{
    [SerializeField] private Exercise exercise;
    [SerializeField] private Transform spawnPosition, middlePosition, endPosition;
    private Glass currentGlass;
    private GameObject newGlass;
    public float movementDuration = 10.0f, score = 0;
    [SerializeField] GameObject prefabGlass;
    private ParticleSystem scoreParticles;
    [SerializeField] RectTransform parentPanel;
    [SerializeField] TMP_Text textScore;
    [SerializeField] PlayerConfig playerConfig;
    [SerializeField] AudioSource glassSource;
    public AudioMixerGroup glassMixerGroup;

    #region Pitch Visualizer
    public AudioSource audioSource;
    public AudioPitchEstimator estimator;
    public LineRenderer lineFrequency;
    public TMP_Text textFrequency;
    public SpriteRenderer avatarRenderer;
    public Sprite neutralSprite, highSprite, mediumSprite, lowSprite;
    #endregion

    #region Arcade Controls
    float timer = 45;
    [SerializeField] TMP_Text timerText;
    [SerializeField] Transform spinner;
    #endregion

    private float frequency = 1;

    public int estimateRate = 30;

    public void Start()
    {
        // Initialize the current data & exercise
        LoadPlayerConfig();

        // Feed the 1st glass to the player so they can sing
        Invoke("NextGlass", 1.0f);

        // call at slow intervals (Update() is generally too fast)
        InvokeRepeating(nameof(UpdateVisualizer), 0, 1.0f / estimateRate);
        glassSource.outputAudioMixerGroup = glassMixerGroup;
    }

    public void FixedUpdate()
    {
        // Compute score & glass dmg as long as there's a glass + the user is singing
        if(currentGlass != null && lineFrequency.positionCount > 0)
        {
            ComputeGlassScore();
        }
    }

    private void Update()
    {
        timer = timer - Time.deltaTime;
        if (timer <= 0)
        {
            playerConfig.maxTimedScore = score;
            SceneManager.LoadScene(3); // End. Go to Highscore
        }
        timerText.text = Mathf.Round(timer) + "s";
        spinner.Rotate(0, 0, -1*(30/timer));
    }

    public void LoadPlayerConfig()
    {
        // Load the user's frequency range and selected exercise
        estimator.frequencyMin = (int) playerConfig.minSingingFreq;
        estimator.frequencyMax = (int) playerConfig.maxSingingFreq;

        exercise = playerConfig.selectedExercise;
        exercise.Reset();
        exercise.RandomizeOrder();
    }

    public void NextGlass()
    {
        currentGlass = exercise.NextGlass();

        // If no more Glasses, we restart the exercise!
        if (currentGlass == null)
        {
            Debug.Log("Reset");
            exercise.Reset();
            exercise.RandomizeOrder();
            NextGlass();
            return;
        }

        // IF the glass frequency is outside of the user's vocal range, it's skipped
        if(currentGlass.frequencyBreak > playerConfig.maxSingingFreq || currentGlass.frequencyBreak < playerConfig.minSingingFreq)
        {
            Debug.Log("Skipped");
            NextGlass();
            return;
        }

        // Make glass appear 
        // TODO prettier animation
        
        if(newGlass == null)
        {
            newGlass = Instantiate(prefabGlass, parentPanel);
        }

        currentGlass.toughness = currentGlass.maxToughness;
        newGlass.GetComponent<Image>().sprite = currentGlass.sprite;
        scoreParticles = newGlass.gameObject.GetComponentInChildren<ParticleSystem>();

        // Move the glass
        newGlass.transform.position = spawnPosition.position;

        newGlass.transform.DOMove(endPosition.position, movementDuration).SetEase(Ease.Linear).OnComplete(() => { GlassEnd(); timer -= 3; }).SetTarget(newGlass); // Go to end
        //newGlass.transform.DOMove(middlePosition.position, movementDuration / 5).SetTarget(newGlass); // Go to middle

        // TODO end animation

        // scale glass according to pitch, Ideally scaling should be minimal, and sprite should be sized according to pitch.
        // UI is scaled to 1920x1080. 5/6 of 1080 is 720
        var freqToHeight = Map(currentGlass.frequencyBreak, estimator.frequencyMin, estimator.frequencyMax, 0, 720); 
        newGlass.GetComponent<RectTransform>().sizeDelta = new Vector2(freqToHeight, freqToHeight);

        newGlass.GetComponent<Button>().onClick.RemoveAllListeners();
        newGlass.GetComponent<Button>().onClick.AddListener(() => GlassClicked(currentGlass.sound));
        GlassClicked(currentGlass.sound);
    }

    public void GlassClicked(AudioClip glassSound)
    {
        // Ding!
        glassSource.PlayOneShot(glassSound);
    }

    public void GlassEnd()
    {
        // We rise the stakes
        // We remove the current glass and feed the next one (if any)
        DOTween.Kill(newGlass);
        NextGlass();
    }

    public void ComputeGlassScore()
    {

        float maxDistance = 15f;
        float maxScore = 4f;

        float voicePrecision = Mathf.Abs(currentGlass.frequencyBreak - frequency);

        if (voicePrecision < maxDistance)
        {
            // Linearly interpolate score: 0 at 15Hz, 1 at 0Hz
            float normalized = 1f - (voicePrecision / maxDistance);
            float scoreToAdd = normalized * maxScore;

            currentGlass.toughness -= 0.02f;
            score += Mathf.Round(scoreToAdd);

            // Interpolate color:
            if (normalized < 0.66f)
            {
                // From red to yellow
                float t = normalized / 0.66f; // Normalize to 0–1 in this range
                scoreParticles.startColor = Color.Lerp(Color.red, Color.yellow, t);
            }
            else
            {
                // From yellow to green
                float t = (normalized - 0.66f) / (1f - 0.66f); // Normalize to 0–1 in this range
                scoreParticles.startColor = Color.Lerp(Color.yellow, Color.green, t);
            }

            scoreParticles.Play();

            if (currentGlass.toughness <= 0)
            {
                score += 100; // 100 score per broken glass
                timer += 3; // We gain extra time
                movementDuration = movementDuration * 0.9f; // We accelerate the glasses
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
            avatarRenderer.sprite = neutralSprite;
        }
        else
        {
            DOTween.To(() => frequency, x => frequency = x, newFrequency, 0.5f); // Smooth frequency visualization

            // indicate the frequency with LineRenderer
            var cam = Camera.main;
            var freqToHeight = Map(frequency, estimator.frequencyMin, estimator.frequencyMax, cam.scaledPixelHeight/6, cam.scaledPixelHeight-(cam.scaledPixelHeight/6));

            lineFrequency.positionCount = 3;
            lineFrequency.SetPosition(0, cam.ScreenToWorldPoint(new Vector3(0, freqToHeight, cam.nearClipPlane))); // Starting the line at the left side
            lineFrequency.SetPosition(1, cam.ScreenToWorldPoint(new Vector3(cam.scaledPixelWidth / 2, freqToHeight, cam.nearClipPlane))); // Middle of the screen
            lineFrequency.SetPosition(2, cam.ScreenToWorldPoint(new Vector3(cam.scaledPixelWidth, freqToHeight, cam.nearClipPlane))); // Ending the line at the right side

            // Display frequency and name of note
            textFrequency.text = "Frequency:\r\n" + frequency + " HZ\r\n" + GetNameFromFrequency(frequency);

            // Assign different faces in top 3rd, middle 3rd and bottom 3rd
            switch (freqToHeight / cam.scaledPixelHeight)
            {
                case > 0.66f:
                    avatarRenderer.sprite = highSprite;
                    break;
                case > 0.33f:
                    avatarRenderer.sprite = mediumSprite;
                    break;
                default:
                    avatarRenderer.sprite = lowSprite;
                    break;
            }
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
