using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu]
public class SoundSystem : ScriptableObject
{
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string VolumeVariableName;

    private float Volume { get; set; } = 1;

    public void SetVolume(float v)
    {
        Volume = v;
        var volumeLvl = v > float.Epsilon ? 20 * Mathf.Log10(Volume) : -144f;
        _audioMixer.SetFloat(VolumeVariableName, volumeLvl);
    }

    public float GetVolume()
    {
        return Volume;
    }

    public bool IsPlayingClip(AudioClip clip)
    {
        if (_musicAudio == null)
        {
            return false;
        }

        return _musicAudio.clip == clip;
    }
    
    private AudioSource _musicAudio;
    public void PlayMusicClip(AudioClip clip, float fadeTime = 0.5f)
    {
        if (_musicAudio == null)
        {
            _musicAudio = (new GameObject()).AddComponent<AudioSource>();
            GameObject.DontDestroyOnLoad(_musicAudio);
            _musicAudio.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("Music")[0];
        }

        _musicAudio.DOFade(0f, 0.5f)
            .OnComplete(() =>
            {
                _musicAudio.clip = clip;
                _musicAudio.loop = true;
                _musicAudio.Play();

                _musicAudio.DOFade(1f, fadeTime);
            });
    }

    public void FadeCurrentMusic(float fadeTime)
    {
        if (_musicAudio == null)
        {
            return;
        }

        _musicAudio.DOFade(0f, fadeTime);
    }

    private readonly List<AudioSource> _oneShotAudio = new ();
    public void PlayOneShot(AudioClip clip, float pitch = 1f)
    {
        var oneShotAudio = _oneShotAudio.Find(source => source.isPlaying == false);
        if (oneShotAudio == null)
        {
            oneShotAudio = (new GameObject()).AddComponent<AudioSource>();
            DontDestroyOnLoad(oneShotAudio);
            oneShotAudio.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("Sfx")[0];
            _oneShotAudio.Add(oneShotAudio);
        }

        oneShotAudio.pitch = pitch;
        oneShotAudio.PlayOneShot(clip, 1f);
    }
}
