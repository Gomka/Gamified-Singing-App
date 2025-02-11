using UnityEngine;
using DG.Tweening;

public class VoiceChallengeController : MonoBehaviour
{
    [SerializeField] private Exercise exercise;
    [SerializeField] private Transform spawnPosition;
    private Glass currentGlass;

    public void Start()
    {
        // Start the scene
        // Initialize the current exercise (quizas diferentes execises viven en diferentes scenes, con su arte y mood)

        // Feed the 1st glass to the player so they can sing
        NextGlass();
    }

    public void NextGlass()
    {
        currentGlass = exercise.NextGlass();

        // If no more Glasses, win!
        if (currentGlass == null)
        {
            // WIN ~ SceneManager.LoadScene(0); 
        }

        // Make glass appear 
        // Play glass sound as reference
        // Clear up any previous data needed

        GameObject newGlass = new GameObject();

        newGlass.AddComponent<Rigidbody>();
        newGlass.AddComponent<SpriteRenderer>().sprite = currentGlass.sprite;
        newGlass.transform.position = spawnPosition.position;

    }


}
