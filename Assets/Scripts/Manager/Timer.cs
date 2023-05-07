using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    public float setTime;
    int min;
    float sec;
    [SerializeField] TMP_Text CntDownText;
    [SerializeField] GameManager GM;

    // Start is called before the first frame update
    void Start()
    {
        min = (int)(setTime -1);
        sec = 59;
        CntDownText.text = setTime.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (sec > 0)
        {
            sec -= Time.deltaTime;
        }
        else
        {
            min -= 1;
            sec = 59;
        }
        if(min < 0)
        {
            Time.timeScale = 0;
        }

        CntDownText.text = min.ToString() + " : " + Mathf.Round(sec).ToString();

    }
}
