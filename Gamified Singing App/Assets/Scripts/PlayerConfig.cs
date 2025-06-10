using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Scriptable Objects/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public float minSingingFreq = 200, maxSingingFreq = 600;
    public Exercise selectedExercise;
    public float maxScore = 0;
    public float maxTimedScore = 0;

    public void ResetUserScore()
    {
        maxScore = 0;
        maxTimedScore = 0;
    }
}
