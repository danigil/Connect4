using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioClip DropSound;
    public AudioClip ClickSound;

    AudioSource SFX;
    AudioSource Music;

    private void Awake()
    {
        SFX = GameObject.FindGameObjectWithTag("SFX").GetComponent<AudioSource>();
        Music = GameObject.FindGameObjectWithTag("Music").GetComponent<AudioSource>();

        //foreach (Button b in GameObject.FindObjectsOfType<Button>()) b.onClick.AddListener(() => { MouseClickSound(); });
    }

    public void SetSFX(float level)
    {
        float val = level;
        SFX.volume = val;
    }

    public void SetMusic(float level)
    {
        float val = Mathf.Log10(level) * 20;
        val = level;
        Debug.Log($"Music slider, {val}");
        Music.volume = val;
    }

    public void MouseClickSound()
    {
        PlaySFX("click");
    }

    public void PlaySFX(string sound)
    {
        switch (sound.ToLower())
        {
            case "drop":
                SFX.PlayOneShot(DropSound);
                break;
            case "click":
                SFX.PlayOneShot(ClickSound);
                break;
            default:
                SFX.PlayOneShot(DropSound);
                break;
        }
    }
}
