using UnityEngine;

[CreateAssetMenu(fileName = "Glass", menuName = "Scriptable Objects/Glass")]
public class Glass : ScriptableObject
{
    [Range(40, 2000)]
    public float frequencyBreak;
    [Range(0.5f, 3)]
    public float maxToughness;
    public float toughness;
    public AudioClip sound; // Morphine (percussion 12)
    public Sprite sprite;
}
