using UnityEngine;
using DG.Tweening;

public class VoiceChallengeController : MonoBehaviour
{
    [SerializeField] private Exercise exercise;
    [SerializeField] private Transform spawnPosition, endPosition;
    private Glass currentGlass;
    public float movementDuration = 20.0f;

    public void Start()
    {
        // Start the scene
        // Initialize the current exercise (quizas diferentes execises viven en diferentes scenes, con su arte y mood)

        // Feed the 1st glass to the player so they can sing
        exercise.Reset();
        //NextGlass();
    }

    public void NextGlass()
    {
        currentGlass = exercise.NextGlass();

        // If no more Glasses, win!
        if (currentGlass == null)
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
        newGlass.transform.DOMove(endPosition.position, movementDuration).OnComplete(() => GlassDone(newGlass));
    }

    public void GlassDone(GameObject go)
    {
        Debug.Log("reached the end");
        GameObject.Destroy(go);
    }
}
