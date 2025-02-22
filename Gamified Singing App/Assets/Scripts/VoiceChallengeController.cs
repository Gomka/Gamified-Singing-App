using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Audio;
using UnityEngine.Timeline;

public class VoiceChallengeController : MonoBehaviour
{
    [SerializeField] private Exercise exercise;
    [SerializeField] private Transform spawnPosition, middlePosition, endPosition;
    private Glass currentGlass;
    public float movementDuration = 20.0f;
    //[SerializeField] GameObject prefabGlass;
    //[SerializeField] RectTransform parentPanel;

    #region Pitch Visualizer
    public AudioSource audioSource;
    public AudioPitchEstimator estimator;
    public LineRenderer lineSRH;
    public LineRenderer lineFrequency;
    public Transform marker;
    public TextMesh textFrequency;
    public TextMesh textMin;
    public TextMesh textMax;
    #endregion

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

    public void NextGlass()
    {
        currentGlass = exercise.NextGlass();

        // If no more Glasses, win!
        if (currentGlass == null)  // AND all present glasses are won
        {
            Debug.Log("Sacabao");
            // WIN ~ SceneManager.LoadScene(0);
            return;
        }

        // Make glass appear 
        // TODO prettier animation

        GameObject newGlass = new GameObject();
        newGlass.AddComponent<SpriteRenderer>().sprite = currentGlass.sprite;

        // Move the glass
        newGlass.transform.position = spawnPosition.position;
        
        //newGlass.transform.DOMove(endPosition.position, movementDuration).SetEase(Ease.Linear).OnComplete(() => GlassEnd(newGlass)); // Go to end
        newGlass.transform.DOMove(middlePosition.position, movementDuration / 5); // Go to middle

        // TODO end animation
        // TODO scale glass according to pitch, Ideally scaling should be minimal, and sprite should be sized according to pitch. Unlikely due to aspect ratios.
        // TODO BIG ONE: make pitch interact with glass
        // Y mil cosas mas que saldrán, pero eso pa luego

        //newGlass.AddComponent<Button>().onClick.AddListener(() => GlassClicked(currentGlass.sound));
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

    void UpdateVisualizer()
    {
        // estimate the fundamental frequency
        var frequency = estimator.Estimate(audioSource);

        Debug.Log(frequency.ToString());

        // visualize SRH score
        var srh = estimator.SRH;
        var numPoints = srh.Count;
        var positions = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            var position = (float)i / numPoints - 0.5f;
            var value = srh[i] * 0.005f;
            positions[i].Set(position, value, 0);
        }
        lineSRH.positionCount = numPoints;
        lineSRH.SetPositions(positions);

        // visualize fundamental frequency
        if (float.IsNaN(frequency))
        {
            // hide the line when it does not exist
            lineFrequency.positionCount = 0;
        }
        else
        {
            var min = estimator.frequencyMin;
            var max = estimator.frequencyMax;
            var position = (frequency - min) / (max - min) - 0.5f;

            // indicate the frequency with LineRenderer
            lineFrequency.positionCount = 2;
            lineFrequency.SetPosition(0, new Vector3(position, +1, 0));
            lineFrequency.SetPosition(1, new Vector3(position, -1, 0));

            // indicate the latest frequency with TextMesh
            marker.position = new Vector3(position, 0, 0);
            textFrequency.text = string.Format("{0}\n{1:0.0} Hz", GetNameFromFrequency(frequency), frequency);
        }

        // visualize lowest/highest frequency
        textMin.text = string.Format("{0} Hz", estimator.frequencyMin);
        textMax.text = string.Format("{0} Hz", estimator.frequencyMax);
    }


    // frequency -> pitch name
    string GetNameFromFrequency(float frequency)
    {
        var noteNumber = Mathf.RoundToInt(12 * Mathf.Log(frequency / 440) / Mathf.Log(2) + 69);
        string[] names = {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
        };
        return names[noteNumber % 12];
    }
}
