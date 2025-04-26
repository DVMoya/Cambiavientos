using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{

    [Header("Settings")]
    [Range(0.01f, 10.0f)]   public float MouseSpeedVertical   = 3.0f;
    [Range(0.01f, 1000.0f)] public float MouseSpeedHorizontal = 300.0f;
    [Range(0.1f, 90.0f)] public float FOV = 40.0f;
    public Vector4Parameter Gamma;

    [Header("References")]
    public CinemachineFreeLook FreeLookCamera;
    public Volume Volume;


    private LiftGammaGain liftGammaGain;

    private void Start()
    {
        LoadSettings();
        UpdateSettings();
    }

    private void Update()
    {
        UpdateSettings();
    }

    private void OnApplicationQuit()
    {
        // Save Settings
    }

    private void LoadSettings()
    {
        // cargar del archivo json donde tengo los ajustes guardados

        // cambiar los ajustes predefinidos a los ajustes guardados


        // actualizar las variables del GameManager
        if (Volume.profile.TryGet<LiftGammaGain>(out liftGammaGain))
        {
            Gamma = liftGammaGain.gamma;
        }
    }

    private void UpdateSettings()
    {
        SetControlSettings();
        SetCameraSettings();
    }

    private void SetControlSettings()
    {
        /*
        La velocidad de la cámara en el eje y es 100 veces menor

        m_YAxis.m_MaxSpeed = MouseSpeed / _mouseSpeedMod  --> 10 veces menor
        m_XAxis.m_MaxSpeed = MouseSpeed * _mouseSpeedMod  --> 10 veces mayor

        MouseSpeed será un rango de 0 a 10 --> [0..10]f
        */

        FreeLookCamera.m_YAxis.m_MaxSpeed = MouseSpeedVertical;
        FreeLookCamera.m_XAxis.m_MaxSpeed = MouseSpeedHorizontal;
    }

    private void SetCameraSettings()
    {
        //Volume.GetComponent<LiftGammaGain>().gamma = Gamma;
        if(Volume.profile.TryGet<LiftGammaGain>(out liftGammaGain))
        {
            liftGammaGain.active = true;

            //Vector4 newGamma = ((Vector4)liftGammaGain.gamma);
            //newGamma.w = Gamma;
            liftGammaGain.gamma.SetValue(Gamma);
        }

        FreeLookCamera.m_Lens.FieldOfView = FOV;
    }
}
