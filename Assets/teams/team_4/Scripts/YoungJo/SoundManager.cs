using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Serializable]
    public class SoundEntry
    {
        public SoundID id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("설정")]
    [SerializeField] private AudioSource sfx2DAudioSource;   // 화면 고정/2D용
    [SerializeField] private List<SoundEntry> soundEntries = new List<SoundEntry>();

    // 내부 매핑용
    private readonly Dictionary<SoundID, SoundEntry> soundTable = new Dictionary<SoundID, SoundEntry>();

    private void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 필요하면:
        // DontDestroyOnLoad(gameObject);

        BuildTable();
        ValidateAudioSource();
    }

    private void BuildTable()
    {
        soundTable.Clear();
        foreach (var entry in soundEntries)
        {
            if (entry == null || entry.clip == null) continue;
            if (soundTable.ContainsKey(entry.id))
            {
                Debug.LogWarning($"[SoundManager] 중복 SoundID 등록: {entry.id}");
                continue;
            }
            soundTable.Add(entry.id, entry);
        }
    }

    private void ValidateAudioSource()
    {
        if (sfx2DAudioSource == null)
        {
            // 자동으로 AudioSource 붙이기 (필요 시)
            sfx2DAudioSource = gameObject.AddComponent<AudioSource>();
            sfx2DAudioSource.playOnAwake = false;
            sfx2DAudioSource.spatialBlend = 0f; // 2D
        }
    }

    /// <summary>
    /// UI, 시스템 사운드처럼 카메라 기준으로 들리는 2D 효과음.
    /// </summary>
    public void PlaySFX(SoundID id)
    {
        if (!soundTable.TryGetValue(id, out var entry) || entry.clip == null)
        {
            Debug.LogWarning($"[SoundManager] 등록되지 않은 SoundID: {id}");
            return;
        }

        sfx2DAudioSource.PlayOneShot(entry.clip, entry.volume);
    }

    /// <summary>
    /// 월드 좌표 position에서 재생되는 3D 효과음.
    /// </summary>
    public void PlaySFX3D(SoundID id, Vector3 position)
    {
        if (!soundTable.TryGetValue(id, out var entry) || entry.clip == null)
        {
            Debug.LogWarning($"[SoundManager] 등록되지 않은 SoundID: {id}");
            return;
        }

        // 가장 단순한 방식: AudioSource.PlayClipAtPoint
        AudioSource.PlayClipAtPoint(entry.clip, position, entry.volume);
    }

    public AudioSource PlaySFX3DLoop(SoundID id, Transform attachTo)
    {
        if (!soundTable.TryGetValue(id, out var entry) || entry.clip == null)
        {
            Debug.LogWarning($"[SoundManager] 등록되지 않은 SoundID: {id}");
            return null;
        }

        if (attachTo == null)
        {
            Debug.LogWarning("[SoundManager] PlaySFX3DLoop: attachTo == null");
            return null;
        }

        // attachTo GameObject에 AudioSource 동적으로 붙임
        var src = attachTo.gameObject.AddComponent<AudioSource>();
        src.clip = entry.clip;
        src.volume = entry.volume;
        src.loop = true;
        src.playOnAwake = false;

        // 3D 설정
        src.spatialBlend = 1f;          // 3D
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 1f;
        src.maxDistance = 10f;

        src.Play();

        return src; // 필요하면 나중에 Stop용으로 참조할 수도 있음
    }
}
