using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : Manager<AudioManager> {

    public AudioClip[] Sounds;
    public AudioSource StepsAudio;
    public AudioSource ScrollAudio;

    public bool PlaySteps;
    public float VolumeScroll;
    public float PitchScroll = 1;

    public void PlaySound(string sound) {
        var theSound = Sounds.Find(sound, (s, name) => s.name == name);
        if (theSound != null) {
            theSound.Play();
        }
    }

    void Update() {
        StepsAudio.volume = PlaySteps ? 1 : 0;
        ScrollAudio.volume = VolumeScroll;
        ScrollAudio.pitch = PitchScroll;
    }
}