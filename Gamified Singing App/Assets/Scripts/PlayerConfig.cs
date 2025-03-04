using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Scriptable Objects/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public float minSingingFreq = 300, maxSingingFreq = 300;
    public Exercise selectedExercise;
    public int selectedCosmetic;

    public enum Cosmetics
    {
        Cosmetic1,
        Cosmetic2,
        Cosmetic3,
    }
}
