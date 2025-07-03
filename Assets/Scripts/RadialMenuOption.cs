using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class RadialMenuOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string txt;

    [SerializeField]
    RawImage Icon;

    RectTransform Rect;

    RadialMenu rm;

    private void Start()
    {
        Rect = GetComponent<RectTransform>();
        rm = GetComponentInParent<RadialMenu>();
    }

    public void SetLabel(string pText)
    {
        txt = pText;
    }

    public string GetLabel()
    {
        return (txt);
    }

    public void SetIcon(Texture pIcon)
    {
        Icon.texture = pIcon;
    }

    public Texture GetIcon()
    {
        return (Icon.texture);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Rect.DOComplete();
        Rect.DOScale(Vector3.one * 1.5f, .3f).SetEase(Ease.OutQuad);
        
        rm.SetSelectedWeather(GetLabel());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Rect.DOComplete();
        Rect.DOScale(Vector3.one, .3f).SetEase(Ease.OutQuad);

        rm.SetSelectedWeather("WEATHER_NONE");
    }
}
