using UnityEngine;

[CreateAssetMenu(fileName = "Exercise", menuName = "Scriptable Objects/Exercise")]
public class Exercise : ScriptableObject
{
    public string exerciseName;
    public Glass[] glasses;
    public int index = 0;

    public Glass NextGlass()
    {
        if (index >= glasses.Length)
        {
            return null;
        }

        return glasses[index++];
    }

    public void Reset()
    {
        index = 0;
    }
}
