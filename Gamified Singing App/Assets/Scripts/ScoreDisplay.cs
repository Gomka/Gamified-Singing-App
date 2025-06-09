using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreDisplay: MonoBehaviour
{
    [SerializeField] PlayerConfig playerConfig;
    [SerializeField] TMP_Text scoreText;
    private void Start()
    {
        scoreText.text = "HighScore\r\n" + playerConfig.maxScore;
    }
}
