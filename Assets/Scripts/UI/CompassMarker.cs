using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CompassMarker : MonoBehaviour
{
    public Image MainImage;

    public CanvasGroup CanvasGroup;

    [Header("Enemy element")]
    public Color DefaultColor;

    public Color AltColor;

    [Header("Direction element")]
    public bool IsDirection;

    public TMPro.TextMeshProUGUI TextContent;

    public void Initialize(CompassElement compassElement, string textDirection)
    {
        if (IsDirection && TextContent)
        {
            TextContent.text = textDirection;
        }
    }

    public void DetectTarget()
    {
        MainImage.color = AltColor;
    }

    public void LostTarget()
    {
        MainImage.color = DefaultColor;
    }
}