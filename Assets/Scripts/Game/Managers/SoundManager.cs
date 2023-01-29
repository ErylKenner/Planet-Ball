using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public Sound[] Sounds;
    public bool AllowRepeatPlay = false;

    private void Awake()
    {
        foreach(Sound sound in Sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.Clip;
            sound.source.volume = sound.Volume;
            sound.source.pitch = sound.Pitch;
        }
    }

    public Sound GetSound(string name)
    {
        Sound sound = Array.Find(Sounds, sound => sound.Name == name);

        if (sound == null)
        {
            Debug.LogWarning($"Sound {name} not found");
            return null;
        }

        return sound;
    }

    public void Stop(string name, float fadeOut=0f)
    {
        Sound sound = GetSound(name);

        if (sound == null)
        {
            return;
        }

        if(sound.FadeOut != null)
        {
            return;
        }

        if(fadeOut <= 0)
        {
            sound.source.Stop();
        } else
        {
            sound.FadeOut = FadeOut(sound.source, fadeOut);
            StartCoroutine(sound.FadeOut);
        }
        
    }

    public void Play(string name, float pitchSpread = 0, float volumeGain = 0)
    {
        Sound sound = GetSound(name);

        if (sound == null)
        {
            return;
        }

        if(!AllowRepeatPlay && sound.source.isPlaying)
        {
            //Debug.LogWarning($"Sound {name} is already playing");
            return;
        }

        if(sound.FadeOut != null)
        {
            StopCoroutine(sound.FadeOut);
            sound.FadeOut = null;
        }

        sound.source.pitch = sound.Pitch;
        sound.source.volume = Mathf.Clamp01(sound.Volume + volumeGain * (1-sound.Volume));

        if (pitchSpread > 0)
        {
            float pitchDiff = UnityEngine.Random.Range(-pitchSpread, pitchSpread);
            sound.source.pitch += pitchDiff;
        }

        sound.source.Play();
    }

    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}
