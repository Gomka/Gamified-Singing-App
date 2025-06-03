using TMPro;
using UnityEngine;

public class FreqToLineMovement: MonoBehaviour
{
    [SerializeField] LineRenderer line;
    private TMP_Text text;

    private void Start()
    {
        text = this.gameObject.GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (line.positionCount == 0)
        {
            //text.fontSize = 0;
        } else
        {
            //text.fontSize = 35;
            this.gameObject.transform.position = line.GetPosition(2);
        }
    }

}
