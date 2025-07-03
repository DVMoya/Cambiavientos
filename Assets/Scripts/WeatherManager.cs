using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public void Clear()
    {
        Debug.Log("The weather is CLEAR, the SUN is SHINING...");
    }
    public void Rain()
    {
        Debug.Log("The sky is CLOUDY and RAIN falls uncesantly...");
    }
    public void Snow()
    {
        Debug.Log("The falling SNOW calms your nerves and FREEZES your lungs...");
    }
    public void Storm()
    {
        Debug.Log("A RAGING STORM aproaches, be aware of LIGHTNINS...");
    }
    public void Tornado()
    {
        Debug.Log("DOROTHY is missing, hope the tornado didn`t catch her, or TOTO...");
    }
}
