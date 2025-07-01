using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System;

public class RadialMenu : MonoBehaviour
{
    [SerializeField]
    GameObject OptionPrefab;

    [SerializeField]
    float Radius = 300f;

    [SerializeField]
    List<Texture> Icons;

    Dictionary<string, Action> WeatherDyctionary = new Dictionary<string, Action>()
    {
        {"WEATHER_NONE",    NoWeather},
        {"WEATHER_CLEAR",   Clear},
        {"WEATHER_RAIN",    Rain},
        {"WEATHER_SNOW",    Snow},
        {"WEATHER_STORM",   Storm},
        {"WEATHER_TORNADO", Tornado}
    };
    List<string> WeatherIndex = new List<string>()
    {
        "WEATHER_NONE",
        "WEATHER_CLEAR",
        "WEATHER_RAIN",
        "WEATHER_SNOW",
        "WEATHER_STORM",
        "WEATHER_TORNADO"
    };
    private string selectedWeather = "WEATHER_NONE";

    List<RadialMenuOption> Options;

    // Start is called before the first frame update
    void Start()
    {
        Options = new List<RadialMenuOption>();
    }

    void AddOption(string pLabel, Texture pIcon)
    {
        GameObject option = Instantiate(OptionPrefab, transform);

        RadialMenuOption rmo = option.GetComponent<RadialMenuOption>();
        rmo.SetLabel(pLabel);
        rmo.SetIcon(pIcon);

        Options.Add(rmo);
    }

    public void Open()
    {
        for (int i = 0; i < Icons.Count; i++)
        {
            AddOption(WeatherIndex[i+1], Icons[i]);
        }
        Roundify();
    }

    public void Close()
    {
        for (int i = 0; i < Options.Count; i++)
        {
            RectTransform rect = Options[i].GetComponent<RectTransform>();
            GameObject option = Options[i].gameObject;

            rect.DOScale(Vector3.zero, .25f).SetEase(Ease.OutQuad);
            rect.DOAnchorPos(Vector3.zero, .3f).SetEase(Ease.OutQuad).onComplete =
                delegate ()
                {
                    Destroy(option);
                };
        }

        Options.Clear();
    }

    public void Toggle()
    {
        if (Options.Count > 0)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    void Roundify() // Make the menu round, who would have guessed?????
    {
        float radiansOfSeparation = (Mathf.PI * 2) / Options.Count;
        for (int i = 0;i < Options.Count; i++)
        {
            float x = Mathf.Sin(radiansOfSeparation * i) * Radius;
            float y = Mathf.Cos(radiansOfSeparation * i) * Radius;

            RectTransform rect = Options[i].GetComponent<RectTransform>();

            rect.localScale = Vector3.zero;
            rect.DOScale(Vector3.one, .3f).SetEase(Ease.OutQuad).SetDelay(0.05f);
            rect.DOAnchorPos(new Vector3(x, y, 0), .3f).SetEase(Ease.OutQuad).SetDelay(0.05f);

        }
    }

    public void SetSelectedWeather(string selected)
    {
        selectedWeather = selected;
    }

    public void ShowSelectedWeather()
    {
        WeatherDyctionary[selectedWeather]?.Invoke();
    }

    static void NoWeather()
    {
        Debug.Log("Something's WRONG, the HEAVENS are BROKEN...");
    }
    static void Clear()
    {
        Debug.Log("The weather is CLEAR, the SUN is SHINING...");
    }
    static void Rain()
    {
        Debug.Log("The sky is CLOUDY and RAIN falls uncesantly...");
    }
    static void Snow()
    {
        Debug.Log("The falling SNOW calms your nerves and FREEZES your lungs...");
    }
    static void Storm()
    {
        Debug.Log("A RAGING STORM aproaches, be aware of LIGHTNINS...");
    }
    static void Tornado()
    {
        Debug.Log("DOROTHY is missing, hope the tornado didn`t catch her, or TOTO...");
    }
}
