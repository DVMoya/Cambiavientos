using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System;
using System.ComponentModel;
using UnityEngine.VFX;
using UnityEngine.Rendering;

public class RadialMenu : MonoBehaviour
{
    [Header("CanvasSettings")]
    public static bool canOpen = true;
    private bool waterUp = false;
    // water up --> y = 0f
    // water down --> y = -3f
    private bool spawnLightning = false;

    [SerializeField]
    GameObject OptionPrefab;

    [SerializeField]
    float Radius = 300f;

    [SerializeField]
    List<Texture> Icons;

    public float transitionTime = 4f;
    public float tornadoTransition = 4.5f;

    [Header("ObjectReferences")]
    [SerializeField] WindmillController windmill;
    [SerializeField] VisualEffect tornado;
    LightningRodDetector lightningRodDetector;


    [SerializeField]
    private List<Material> snowMaterials = new List<Material>();
    public string whatIsGround = "Ground";

    public CloudGenerator cloud;
    private Material cloudMaterial;
    /*
    cloudCutoff = 0 --> nubes 100%
    cloudCutoff = .75 --> nubes normales
    baseColor --> no puede tener nada de luz sino la emisión aumenta demasiado
    */
    public Transform waterPosition;
    public GameObject water;
    private Material waterMaterial;
    public Collider waterCollider;

    [Header("Weather Settings")]

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

        // obtengo las referencias a los materiales
        cloudMaterial = cloud.cloudMaterial;
        waterMaterial = water.GetComponent<Renderer>().sharedMaterial;
        AddGroundMaterials();

        lightningRodDetector = FindObjectOfType<LightningRodDetector>();

        waterMaterial.SetFloat("_Freeze", 0f);
        Clear();
        StartCoroutine(ToggleSnowCoverage(false, 0.01f));
    }

    void AddGroundMaterials()
    {
        // Encuentra todos los objetos con el "ground" tag
        GameObject[] groundObjects = GameObject.FindGameObjectsWithTag(whatIsGround);

        foreach (GameObject obj in groundObjects)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Add all materials from the renderer to the list
                foreach (Material mat in renderer.sharedMaterials)
                {
                    if (!snowMaterials.Contains(mat))
                    {
                        snowMaterials.Add(mat);
                    }
                }
            }
        }
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
    }

    public void ShowSelectedWeather()
    {
        if (selectedWeather == previousWeather || selectedWeather == "WEATHER_NONE") { return; }

        if(selectedWeather != "WEATHER_SNOW" ){
            // desactivo la capa de nieve del material para la lista de objetos estáticos
            StartCoroutine(ToggleSnowCoverage(false, transitionTime));
        }

        WeatherDyctionary[selectedWeather]?.Invoke();
    }

    void NoWeather()
    {
        selectedWeather = previousWeather;
        Debug.Log("Something's WRONG, the HEAVENS are BROKEN...");
    }
    void Clear()
    {
        Debug.Log("The weather is CLEAR, the SUN is SHINING...");
        if (previousWeather != "WEATHER_CLEAR"){
            spawnLightning = false;

            StartCoroutine(LerpCloudCutoff(0.75f, transitionTime));
            StartCoroutine(LerpAmbienLighting(true, clearLight, transitionTime));

            // desactiva las particulas de lluvia y nieve
            if (rainPS.isPlaying)  ToggleRain();
            if (snowPSa.isPlaying) ToggleSnow();

            // hago que el molino pase a idle
            StartCoroutine(WindmillSpeed(false, transitionTime));

            // bajo el nivel del agua (se evapora)
            if(waterUp) 
                StartCoroutine(ChangeWaterLevel(waterPosition.position.y-3f, transitionTime));
        }
    }
    void Rain()
    {
        Debug.Log("The sky is CLOUDY and RAIN falls uncesantly...");
        if (previousWeather != "WEATHER_RAIN"){
            spawnLightning = false;

            StartCoroutine(LerpCloudCutoff(0.0f, transitionTime));
            StartCoroutine(LerpAmbienLighting(false, rainLight, transitionTime));

            // desactivo los copos de nieve antes de que empiece a llover
            if (snowPSa.isPlaying) ToggleSnow();
            if (!rainPS.isPlaying) ToggleRain();

            // hago que el molino pase a idle
            StartCoroutine(WindmillSpeed(false, transitionTime));

            // Sube el nivel del agua con la lluvia
            if (!waterUp) StartCoroutine(ChangeWaterLevel(waterPosition.position.y+3f, transitionTime));
        }
    }
    void Snow()
    {
        Debug.Log("The falling SNOW calms your nerves and FREEZES your lungs...");
        if (previousWeather != "WEATHER_SNOW"){
            spawnLightning = false;

            StartCoroutine(LerpCloudCutoff(0.0f, transitionTime));
            StartCoroutine(LerpAmbienLighting(false, snowLight, transitionTime));

            // desactivo la lluvia antes de que empiece a nevar
            if (rainPS.isPlaying) ToggleRain();
            if (!snowPSa.isPlaying) ToggleSnow();

            // hago que el molino pase a idle
            StartCoroutine(WindmillSpeed(false, transitionTime));

            // activo la capa de nieve del material para la lista de objetos estáticos
            StartCoroutine(ToggleSnowCoverage(true, transitionTime));
        }
    }
    void Storm()
    {
        Debug.Log("A RAGING STORM aproaches, be aware of LIGHTNINS...");
        if (previousWeather != "WEATHER_STORM"){
            spawnLightning = true;

            StartCoroutine(LerpCloudCutoff(0.0f, transitionTime));
            StartCoroutine(LerpAmbienLighting(false, stormLight, transitionTime));

            // desactivo los copos de nieve antes de que empiece a llover
            if (snowPSa.isPlaying) ToggleSnow();
            if (!rainPS.isPlaying) ToggleRain();

            // hago que el molino pase a idle
            StartCoroutine(WindmillSpeed(false, transitionTime));

            // inicio el ciclo que hace que aparezcan rayos en los pararayos
            StartCoroutine(SpawnLightning());

            // Sube el nivel del agua con la lluvia
            if (!waterUp) StartCoroutine(ChangeWaterLevel(waterPosition.position.y+3f, transitionTime));
        }
    }
    void Tornado()
    {
        Debug.Log("DOROTHY is missing, hope the tornado didn`t catch her, or TOTO...");
        if (previousWeather != "WEATHER_TORNADO"){
            spawnLightning = false;

            StartCoroutine(SpawnTornado(tornadoTransition));
            StartCoroutine(LerpCloudCutoff(0.0f, transitionTime - 3f));
            StartCoroutine(LerpAmbienLighting(false, tornadoLight, transitionTime - 3f));

            // incluso si puede llover durante un tornado, me gusta como se ve sin partículas
            if (rainPS.isPlaying) ToggleRain();
            if (snowPSa.isPlaying) ToggleSnow();

            // hago que el molino se ponga a mil
            StartCoroutine(WindmillSpeed(true, transitionTime - 3f));
        }
    }

    IEnumerator LerpCloudCutoff(float newCutoff, float duration)
    {
        // no quiero que se pueda usar el cambiavientos mientras hay una transición climática
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

        if(selectedWeather != "WEATHER_TORNADO" || !(selectedWeather == "WEATHER_NONE" && previousWeather == "WEATHER_TORNADO"))
        {
            // ahora que ha terminado la transición sí que se puede volver a usar el cambiavientos
            canOpen = true;
        }
        

        Debug.Log("The sky RUMBLES anew...");
    }
    IEnumerator LerpAmbienLighting(bool clear, float lightLevel, float duration)
    {
        float time = 0f;

        // Cuáles son los nuevos valores?
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

            // Cambio el valor del gradiente y la intensidad lumínica cada frame
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

    IEnumerator ToggleSnowCoverage(bool snowCoverage, float duration)
    {
        float initialCutoff;
        float targetCutoff = 1f;
        if(snowCoverage) targetCutoff = 0.4f;
        // snow cut off == 0.4 --> casi todo cubierto de nieve
        // snow cut off == 1   --> nada cubierto de nieve
        foreach(Material mtr in snowMaterials)
        {
            if(mtr.HasProperty("_SnowCutoff"))
            {
                initialCutoff = mtr.GetFloat("_SnowCutoff");

                StartCoroutine(TransitionSnow(mtr, initialCutoff, targetCutoff, duration));
            }
        }

        float time = 0f;
        float initialFreeze = waterMaterial.GetFloat("_Freeze");
        float targetFreeze = initialFreeze;
        waterCollider.enabled = true;
        if (selectedWeather == "WEATHER_SNOW")
        {
            targetFreeze = 1f;
        } else if(selectedWeather == "WEATHER_CLEAR")
        {
            targetFreeze = 0f;
        }
        if(waterUp && targetFreeze == 1f)
        {
            waterCollider.enabled = false;
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            waterMaterial.SetFloat("_Freeze", Mathf.Lerp(initialFreeze, targetFreeze, t));

            yield return null;
        }
    }

    IEnumerator TransitionSnow(Material mtr, float initialCutoff, float targetCutoff, float duration)
    {
        float time = 0f;

        while(time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            mtr.SetFloat("_SnowCutoff", Mathf.Lerp(initialCutoff, targetCutoff, t));

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
            // si está activo quiero parar las partículas, darle un segundo o dos y despues desactivarlo
            ps.Stop();

            while (ps.IsAlive())
            {
                yield return null;
            }

            ps.gameObject.SetActive(false);
        } else
        {
            // si no está activo quiero activarlo y darle a play inmediatamente
            ps.gameObject.SetActive(true);
            ps.Play();
        }

        yield return null;
    }

    IEnumerator ChangeWaterLevel(float targetLvl, float duration)
    {
        if (waterMaterial.GetFloat("_Freeze") < 1f)
        {
            waterUp = !waterUp;

            float time = 0f;
            Vector3 pos = waterPosition.position;
            float initialLvl = pos.y;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;

                waterPosition.position = new Vector3(pos.x, Mathf.Lerp(initialLvl, targetLvl, t), pos.z);

                yield return null;
            }
        }
    }

    IEnumerator SpawnLightning()
    {
        while(spawnLightning)
        {
            yield return new WaitForSeconds(4f);

            if (lightningRodDetector.lightningRodsInRange.Count > 0)
            {
                int rand = UnityEngine.Random.Range(0, lightningRodDetector.lightningRodsInRange.Count);
                GameObject target = lightningRodDetector.lightningRodsInRange[rand];
                target.GetComponentInChildren<VisualEffect>().Play();

                // no todos los pararrayos tienen porque poder activar algo, pero los que pueden lo hacen
                // en el caso de esta demo son pistones que levantan y bajan plataformas
                Activator activator = target.GetComponent<Activator>();
                if(activator != null && !activator.isMoving) 
                {
                    activator.isMoving = true;
                    activator.SoftMove(transitionTime);
                }
            }
        }
    }

    IEnumerator SpawnTornado(float duration)
    {
        tornado.Play();

        float time = 0f;
        while(time < duration)
        {
            time += Time.deltaTime;
            // aun esta la animacion del tornado
            yield return null;
        }

        selectedWeather = "WEATHER_CLEAR";
        ShowSelectedWeather();
    }
}
