using UnityEngine;

public class ExerciseSelector: MonoBehaviour {

    public PlayerConfig playerConfig;

    private void Start()
    {
        ShowMenuOptions();
    }

    public void SetExercise(Exercise exercise)
    {
        playerConfig.selectedExercise = exercise;
    }

    public void ShowMenuOptions()
    {

    }

    public void ShowExercises()
    {

    }
}
