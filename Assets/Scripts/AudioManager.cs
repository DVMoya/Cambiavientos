using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;

    public static AudioManager instance;

    void Awake()
    {
        if(instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    // llama a esta funciond desde cualquier lado con:
    // FindObjectOfType<AudioManager>().Play("[EL NOMBRE DEL CLIP]");
    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound  => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("El audio " + name + " no se encuentra en la lista de audios.");
            return;
        }

        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("El audio " + name + " no se encuentra en la lista de audios.");
            return;
        }

        s.source.Stop();
    }

    public void ChangeAmbientSound(string name)
    {
        if (name != "AMBIENCE_CLEAR")   Stop("AMBIENCE_CLEAR");
        if (name != "AMBIENCE_RAIN")    Stop("AMBIENCE_RAIN");
        if (name != "AMBIENCE_STORM")   Stop("AMBIENCE_STORM");

        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("El audio " + name + " no se encuentra en la lista de audios.");
            return;
        }

        if (!s.source.isPlaying) Play(name);
    }
}
