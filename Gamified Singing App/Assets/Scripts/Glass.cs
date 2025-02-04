using UnityEngine;

[CreateAssetMenu(fileName = "Glass", menuName = "Scriptable Objects/Glass")]
public class Glass : ScriptableObject
{
    public float frequencyBreak, toughness;
    public AudioClip sound;
    public Sprite sprite;
}
