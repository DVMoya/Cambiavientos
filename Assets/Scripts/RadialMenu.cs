using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System;
using System.ComponentModel;

public class RadialMenu : MonoBehaviour
{
    [HideInInspector]
    public enum WeatherType
    {
        Clear,
        Rain,
        Snow,
        Storm,
        Tornado
    }

    [HideInInspector] public static WeatherType currentWeather;

    [Header("CanvasSettings")]
    public static bool canOpen = true;

    [SerializeField]
    GameObject OptionPrefab;

    [SerializeField]
    float Radius = 300f;

    [SerializeField]
    List<Texture> Icons;

    public float transitionTime = 4f;

    [Header("ObjectReferences")]
    [SerializeField] WindmillController windmill;

    [Header("Weather Settings")]
    public Material cloudMaterial;
    /*
    cloudCutoff = 0 --> nubes 100%
    cloudCutoff = .75 --> nubes normales
    baseColor --> no puede tener nada de luz sino la emisi�n aumenta demasiado
    */

    [SerializeField] ParticleSystem rainPS;
    [SerializeField] ParticleSystem rainPSripple;
    [SerializeField] ParticleSystem snowPSa;
    [SerializeField] ParticleSystem snowPSb;
    [SerializeField] ParticleSystem snowPSc;

    [Header("Lighting Settings")]
    public float clearLight = 1f;
    public float rainLight = 0.5f;
    public float snowLight = 0.75f;
    public float stormLight = 0.3f;
    public float tornadoLight = 0.3f;

    public Color clearSkyColor;
    public Color clearEquatorColor;
    public Color clearGroundColor;

    public Color cloudySkyColor;
    public Color cloudyEquatorColor;
    public Color cloudyGroundColor;

    Dictionary<string, Action> WeatherDyctionary;
    List<string> WeatherIndex = new List<string>()
    {
        "WEATHER_NONE",
        "WEATHER_CLEAR",
        "WEATHER_RAIN",
        "WEATHER_SNOW",
        "WEATHER_STORM",
        "WEATHER_TORNADO"
    };
    private string selectedWeather  = "WEATHER_NONE";
    private string previousWeather = "WEATHER_NONE";

    List<RadialMenuOption> Options;

    // Start is called before the first frame update
    void Start()
    {
        Options = new List<RadialMenuOption>();

        WeatherDyctionary = new Dictionary<string, Action>()
        {
            {"WEATHER_NONE",    NoWeather},
            {"WEATHER_CLEAR",   Clear},
            {"WEATHER_RAIN",    Rain},
            {"WEATHER_SNOW",    Snow},
            {"WEATHER_STORM",   Storm},
            {"WEATHER_TORNADO", Tornado}
        };

        Clear();

        currentWeather = WeatherType.Clear;
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
        previousWeather = selectedWeather;
        selectedWeather = selected;

        currentWeather = selectedWeather switch
        {
            "WEATHER_CLEAR" => WeatherType.Clear,
            "WEATHER_RAIN" => WeatherType.Rain,
            "WEATHER_SNOW" => WeatherType.Snow,
            "WEATHER_STORM" => WeatherType.Storm,
            "WEATHER_TORNADO" => WeatherType.Tornado,
            _ => WeatherType.Clear,
        };
    }

    public void ShowSelectedWeather()
    {
        if (selectedWeather == previousWeather) { return; }

        WeatherDyctionary[selectedWeather]?.Invoke();
    }

    void NoWeather()
    {
        Debug.Log("Something's WRONG, the HEAVENS are BROKEN...");
    }
    void Clear()
    {
        Debug.Log("The weather is CLEAR, the SUN is SHINING...");
        if (previousWeather != "WEATHER_CLEAR"){
            StartCoroutine(LerpCloudCutoff(0.75f, transitionTime));
            StartCoroutine(LerpAmbienLighting(true, clearLight, transitionTime));

            // desactiva las particulas de lluvia y nieve
            if (rainPS.isPlaying)  ToggleRain();
            if (snowPSa.isPlaying) ToggleSnow();

            // hago que el molino pase a idle
            StartCoroutine(WindmillSpeed(false, transitionTime));
        }
    }
    void Rain()
    {
        Debug.Log("The sky is CLOUDY and RAIN falls uncesantly...");
        if (previousWeather != "WEATHER_RAIN"){
            StartCoroutine(LerpCloudCutoff(0.0f, transitionTime));
            StartCoroutine(LerpAmbienLighting(false, rainLight, transitionTime));

            // desactivo los copos de nieve antes de que empiece a llover
            if (snowPSa.isPlaying) ToggleSnow();
            if (!rainPS.isPlaying) ToggleRain();

            // hago que el molino pase a idle
            StartCoroutine(WindmillSpeed(false, transitionTime));
        }
    }
    void Snow()
    {
        Debug.Log("The falling SNOW calms your nerves and FREEZES your lungs...");
        if (previousWeather != "WEATHER_SNOW"){
            StartCoroutine(LerpCloudCutoff(0.0f, transitionTime));
            StartCoroutine(LerpAmbienLighting(false, snowLight, transitionTime));

            // desactivo la lluvia antes de que empiece a nevar
            if (rainPS.isPlaying) ToggleRain();
            if (!snowPSa.isPlaying) ToggleSnow();

            // hago que el molino pase a idle
            StartCoroutine(WindmillSpeed(false, transitionTime));
        }
    }
    void Storm()
    {
        Debug.Log("A RAGING STORM aproaches, be aware of LIGHTNINS...");
        if (previousWeather != "WEATHER_STORM"){
            StartCoroutine(LerpCloudCutoff(0.0f, transitionTime));
            StartCoroutine(LerpAmbienLighting(false, stormLight, transitionTime));

            // desactivo los copos de nieve antes de que empiece a llover
            if (snowPSa.isPlaying) ToggleSnow();
            if (!rainPS.isPlaying) ToggleRain();

            // hago que el molino pase a idle
            StartCoroutine(WindmillSpeed(false, transitionTime));
        }
    }
    void Tornado()
    {
        Debug.Log("DOROTHY is missing, hope the tornado didn`t catch her, or TOTO...");
        if (previousWeather != "WEATHER_TORNADO"){
            StartCoroutine(LerpCloudCutoff(0.0f, transitionTime));
            StartCoroutine(LerpAmbienLighting(false, tornadoLight, transitionTime));

            // incluso si puede llover durante un tornado, me gusta como se ve sin part�culas
            if (rainPS.isPlaying) ToggleRain();
            if (snowPSa.isPlaying) ToggleSnow();

            // hago que el molino se ponga a mil
            StartCoroutine(WindmillSpeed(true, transitionTime));
        }
    }

    IEnumerator LerpCloudCutoff(float newCutoff, float duration)
    {
        // no quiero que se pueda usar el cambiavientos mientras hay una transici�n clim�tica
        canOpen = false;

        float time = 0f;

        // cojo el valor inicial
        float startCutoff = cloudMaterial.GetFloat("_CloudCutoff");

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // calcula el nuevo valor durante el frame
            float currentCutoff = Mathf.Lerp(startCutoff, newCutoff, t);
            cloudMaterial.SetFloat("_CloudCutoff", currentCutoff);

            yield return null;
        }

        // ahora que ha terminado la transici�n s� que se puede volver a usar el cambiavientos
        canOpen = true;

        Debug.Log("The sky RUMBLES anew...");
    }
    IEnumerator LerpAmbienLighting(bool clear, float lightLevel, float duration)
    {
        float time = 0f;

        // Cu�les son los nuevos valores?
        Color targetSkyColor = clearSkyColor;
        Color targetEquatorColor = clearEquatorColor;
        Color targetGroundColor = clearGroundColor;
        if (!clear)
        {
            targetSkyColor = cloudySkyColor;
            targetEquatorColor = cloudyEquatorColor;
            targetGroundColor = cloudyGroundColor;
        }

        // Obtengo los valores iniciales
        Color initialSkyColor = RenderSettings.ambientSkyColor;
        Color initialEquatorColor = RenderSettings.ambientEquatorColor;
        Color initialGroundColor = RenderSettings.ambientGroundColor;

        // Cojo el valor actual de la intensidad del sol
        float initialLightLevel = RenderSettings.sun.intensity;

        Debug.Log(initialSkyColor);
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Cambio el valor del gradiente y la intensidad lum�nica cada frame
            RenderSettings.ambientSkyColor = Color.Lerp(initialSkyColor, targetSkyColor, t);
            RenderSettings.ambientEquatorColor = Color.Lerp(initialEquatorColor, targetEquatorColor, t);
            RenderSettings.ambientGroundColor = Color.Lerp(initialGroundColor, targetGroundColor, t);
            RenderSettings.sun.intensity = Mathf.Lerp(initialLightLevel, lightLevel, t);

            yield return null;
        }

        Debug.Log("Light changes with the CLOUDS...");
    }

    IEnumerator WindmillSpeed(bool highOn, float duration)
    {
        float time = 0f;
        float initialSpeed = windmill.velocity;
        float targetSpeed = 10f;
        if (highOn)
        {
            targetSpeed = 600f;
        }

        while(time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            windmill.velocity = Mathf.Lerp(initialSpeed, targetSpeed, t);

            yield return null;
        }

    }

    private void ToggleRain()
    {
        //StartCoroutine(ToggleParticles(rainPSripple));
        StartCoroutine(ToggleParticles(rainPS));
    }
    private void ToggleSnow()
    {
        StartCoroutine(ToggleParticles(snowPSa));
        StartCoroutine(ToggleParticles(snowPSb));
        StartCoroutine(ToggleParticles(snowPSc));
    }

    IEnumerator ToggleParticles(ParticleSystem ps)
    {
        if(ps.gameObject.activeInHierarchy) 
        {
            // si est� activo quiero parar las part�culas, darle un segundo o dos y despues desactivarlo
            ps.Stop();

            while (ps.IsAlive())
            {
                yield return null;
            }

            ps.gameObject.SetActive(false);

        } else
        {
            // si no est� activo quiero activarlo y darle a play inmediatamente
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        yield return null;
    }
}
