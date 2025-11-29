using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

public class BowlSpawner : MonoBehaviour
{
    [Header("Prefab Reference")]
    public GameObject bowlDummyPrefab;
    
    [Header("Spawn Settings")]
    public Vector3 spawnOffset = new Vector3(0, 0.1f, 0);
    
    private List<GameObject> spawnedBowls = new List<GameObject>();

    void Start()
    {
        // MRUK 이벤트 구독
        if (MRUK.Instance != null)
        {
            MRUK.Instance.RoomCreatedEvent.AddListener(OnRoomCreated);
        }
    }

    void OnRoomCreated(MRUKRoom room)
    {
        Debug.Log("Room created, spawning bowls...");
        SpawnBowlsOnOtherObjects(room);
    }

    void SpawnBowlsOnOtherObjects(MRUKRoom room)
    {
        // 기존 bowl 제거
        foreach (var bowl in spawnedBowls)
        {
            if (bowl != null) Destroy(bowl);
        }
        spawnedBowls.Clear();

        // "Other" 레이블 앵커 찾기
        foreach (var anchor in room.Anchors)
        {
            if (anchor.Label == MRUKAnchor.SceneLabels.OTHER)
            {
                SpawnBowlAtAnchor(anchor);
            }
        }

        Debug.Log($"Spawned {spawnedBowls.Count} bowls on 'Other' objects");
    }

    void SpawnBowlAtAnchor(MRUKAnchor anchor)
    {
        GameObject bowl;
        
        if (bowlDummyPrefab != null)
        {
            bowl = Instantiate(bowlDummyPrefab, anchor.transform.position + spawnOffset, Quaternion.identity);
        }
        else
        {
            bowl = new GameObject("Bowl");
            bowl.AddComponent<BowlDummy>();
            bowl.transform.position = anchor.transform.position + spawnOffset;
        }
        
        bowl.transform.parent = anchor.transform;
        spawnedBowls.Add(bowl);
        
        Debug.Log($"Bowl spawned at: {anchor.gameObject.name}");
    }

    void OnDestroy()
    {
        if (MRUK.Instance != null)
        {
            MRUK.Instance.RoomCreatedEvent.RemoveListener(OnRoomCreated);
        }
    }
}