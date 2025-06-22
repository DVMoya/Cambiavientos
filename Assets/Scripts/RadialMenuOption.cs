using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RadialMenuOption : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI Label;

    [SerializeField]
    RawImage Icon;
    
    public void SetLabel(string pText)
    {
        Label.text = pText;
    }

    public void SetIcon(Texture pIcon)
    {
        Icon.texture = pIcon;
    }

    public Texture GetIcon()
    {
        return (Icon.texture);
    }
}
