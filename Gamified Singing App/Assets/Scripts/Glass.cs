using UnityEngine;

[CreateAssetMenu(fileName = "Glass", menuName = "Scriptable Objects/Glass")]
public class Glass : ScriptableObject
{
    [Range(40, 1200)]
    public float frequencyBreak;
    [Range(0.5f, 3)]
    public float toughness;
    public AudioClip sound;
    public Sprite sprite;
}
