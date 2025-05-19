using UnityEngine;

public class ExerciseSelector: MonoBehaviour {

    public PlayerConfig playerConfig;
    public void SetExercise(Exercise exercise)
    {
        playerConfig.selectedExercise = exercise;
    }
}
