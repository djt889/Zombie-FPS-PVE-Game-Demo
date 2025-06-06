using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;

public enum SoundType
{
    MUSIC,      // 背景音乐
    UI,         // 界面音效
    SFX,        // 游戏特效音效
    VOICE,      // 角色语音
    AMBIENT     // 环境音效
}

[System.Serializable]
public class Sound
{
    public string id;          // 音效唯一标识
    public AudioClip clip;     // 音频资源
    
    [Header("基本设置")]
    public SoundType type = SoundType.SFX;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;
    
    [Header("3D音效设置")]
    [Range(0f, 1f)] public float spatialBlend = 0f; // 0=2D, 1=3D
    public float minDistance = 1f;
    public float maxDistance = 500f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("混音器设置")]
    public AudioMixer masterMixer;
    
    [Header("音效库")]
    public List<Sound> soundLibrary = new List<Sound>();
    
    [Header("音量设置")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    
    private Dictionary<string, Sound> soundDict = new Dictionary<string, Sound>();
    private Dictionary<SoundType, AudioSource> audioSources = new Dictionary<SoundType, AudioSource>();
    
    void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Initialize()
    {
        // 创建音频源
        CreateAudioSource(SoundType.MUSIC, "MusicSource");
        CreateAudioSource(SoundType.UI, "UISource");
        CreateAudioSource(SoundType.SFX, "SFXSource");
        CreateAudioSource(SoundType.VOICE, "VoiceSource");
        CreateAudioSource(SoundType.AMBIENT, "AmbientSource");
        
        // 构建音效字典
        foreach (Sound sound in soundLibrary)
        {
            if (!soundDict.ContainsKey(sound.id))
            {
                soundDict.Add(sound.id, sound);
            }
            else
            {
                Debug.LogWarning($"重复的音效ID: {sound.id}");
            }
        }
        
        // 加载音量设置
        LoadVolumeSettings();
    }
    
    private void CreateAudioSource(SoundType type, string name)
    {
        GameObject sourceObj = new GameObject(name);
        sourceObj.transform.SetParent(transform);
        
        AudioSource source = sourceObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        audioSources.Add(type, source);
    }
    
    #region 音量控制
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        masterMixer.SetFloat("MasterVolume", ConvertToDecibel(masterVolume));
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        masterMixer.SetFloat("MusicVolume", ConvertToDecibel(musicVolume));
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        masterMixer.SetFloat("SFXVolume", ConvertToDecibel(sfxVolume));
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }
    
    private float ConvertToDecibel(float volume)
    {
        // 将线性音量转换为分贝值
        return volume <= 0f ? -80f : Mathf.Log10(volume) * 20f;
    }
    
    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }
    
    #endregion
    
    #region 音效播放
    
    public void PlaySound(string id)
    {
        if (soundDict.TryGetValue(id, out Sound sound))
        {
            PlaySound(sound);
        }
        else
        {
            Debug.LogWarning($"找不到音效: {id}");
        }
    }
    
    public void PlaySound(Sound sound)
    {
        if (audioSources.TryGetValue(sound.type, out AudioSource source))
        {
            // 为3D音效创建临时音频源
            if (sound.spatialBlend > 0f)
            {
                PlaySpatialSound(sound);
                return;
            }
            
            source.volume = GetAdjustedVolume(sound);
            source.pitch = sound.pitch;
            source.loop = sound.loop;
            source.clip = sound.clip;
            source.Play();
        }
    }
    
    private void PlaySpatialSound(Sound sound)
    {
        // 为3D音效创建临时音频源
        GameObject tempSource = new GameObject($"TempAudio_{sound.id}");
        AudioSource source = tempSource.AddComponent<AudioSource>();
        
        source.clip = sound.clip;
        source.volume = GetAdjustedVolume(sound);
        source.pitch = sound.pitch;
        source.loop = sound.loop;
        source.spatialBlend = sound.spatialBlend;
        source.minDistance = sound.minDistance;
        source.maxDistance = sound.maxDistance;
        
        source.Play();
        
        // 音效播放完后销毁对象
        if (!sound.loop)
        {
            Destroy(tempSource, sound.clip.length + 0.1f);
        }
    }
    
    private float GetAdjustedVolume(Sound sound)
    {
        float volume = sound.volume;
        
        // 应用全局音量设置
        switch (sound.type)
        {
            case SoundType.MUSIC:
                volume *= musicVolume;
                break;
            case SoundType.UI:
            case SoundType.SFX:
            case SoundType.VOICE:
            case SoundType.AMBIENT:
                volume *= sfxVolume;
                break;
        }
        
        return volume * masterVolume;
    }
    
    public void StopSound(string id)
    {
        // 停止所有该ID的音效
        foreach (var source in audioSources.Values)
        {
            if (source.clip != null && source.clip.name == id)
            {
                source.Stop();
            }
        }
    }
    
    public void StopAllSounds(SoundType type)
    {
        if (audioSources.TryGetValue(type, out AudioSource source))
        {
            source.Stop();
        }
    }
    
    #endregion
    
    #region 背景音乐控制
    
    public void PlayMusic(string id)
    {
        if (soundDict.TryGetValue(id, out Sound music))
        {
            if (audioSources.TryGetValue(SoundType.MUSIC, out AudioSource source))
            {
                // 淡出当前音乐
                if (source.isPlaying)
                {
                    StartCoroutine(FadeOutMusic(source, 1f, () => {
                        PlayNewMusic(source, music);
                    }));
                }
                else
                {
                    PlayNewMusic(source, music);
                }
            }
        }
    }
    
    private void PlayNewMusic(AudioSource source, Sound music)
    {
        source.volume = GetAdjustedVolume(music);
        source.pitch = music.pitch;
        source.loop = music.loop;
        source.clip = music.clip;
        source.Play();
    }
    
    private IEnumerator FadeOutMusic(AudioSource source, float duration, Action onComplete)
    {
        float startVolume = source.volume;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }
        
        source.Stop();
        source.volume = startVolume;
        onComplete?.Invoke();
    }
    
    #endregion
}