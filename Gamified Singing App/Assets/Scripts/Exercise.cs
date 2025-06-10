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
        foreach (var glass in glasses)
        {
            glass.toughness = glass.maxToughness;
        }
    }

    public void RandomizeOrder()
    {
        int n = glasses.Length-1;
        while (n > 1)
        {
            int k = Random.Range(0, n--);
            Glass temp = glasses[n];
            glasses[n] = glasses[k];
            glasses[k] = temp;
        }
    }
}
