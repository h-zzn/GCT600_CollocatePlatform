using UnityEngine;
using Meta.XR.MRUtilityKit;

[System.Serializable]
public struct PersonSpawnOffset
{
    public Vector3 chunhyangOffset;
    public Vector3 sattoOffset;
    public Vector3 mongryongOffset;
}

[System.Serializable]
public struct ButterflySpawnOffset
{
    public Vector3 butterfly1Offset;
    public Vector3 butterfly2Offset;
    public Vector3 butterfly3Offset;
}

public class AnimatedCharacterController : MonoBehaviour
{
    [Header("Character Prefabs")] 
    [SerializeField] private GameObject tigerPrefab;
    [SerializeField] private GameObject[] flowerPrefabs;
    [SerializeField] private GameObject[] personPrefabs;   


    [Header("Settings")]
    [SerializeField] private float distanceFromScreen = 2f;
    [SerializeField] private float spawnOffsetValue = 2f;
    [SerializeField] private float fadeDuration = 2f;

    [SerializeField] private DecalManager decalManager;

    [Header("Butterfly Target Offsets")]
    [SerializeField] private ButterflySpawnOffset butterflyOffsets;

    [Header("Person First Target Offsets")]
    [SerializeField] private PersonSpawnOffset personOffsets;

    [Header("Tiger / Butterfly First Target Offset")]
    [SerializeField] private Vector3 tigerOffset = Vector3.zero;
    // [SerializeField] private Vector3 butterflyOffset = Vector3.zero;



    private GameObject currentCharacter;      // 현재 생성된 캐릭터 Clone
    private MRUKAnchor currentScreenAnchor;
    private GameObject currentFusedBaekja;
    private DecalType currentDecalType;

    private TigerMover tigerMover;

    private void Start()
    {
        if (BaekjaHandler.Instance != null)
            BaekjaHandler.Instance.OnBaekjaCreated += OnFusedBaekjaCreated;
    }

    private void OnDestroy()
    {
        if (BaekjaHandler.Instance != null)
            BaekjaHandler.Instance.OnBaekjaCreated -= OnFusedBaekjaCreated;
    }

    private void OnFusedBaekjaCreated(GameObject fusedBaekja)
    {
        currentFusedBaekja = fusedBaekja;
    }

    // 캐릭터 스폰 공통 함수
    private void SpawnTiger(GameObject prefab, MRUKAnchor anchor)
    {
        if (currentCharacter != null)
            Destroy(currentCharacter);

        currentScreenAnchor = anchor;
        currentCharacter = Instantiate(prefab);

        // TABLE 앵커 가져오기
        MRUKAnchor tableAnchor = MRUKManager.Instance.TableAnchor;
        if (tableAnchor == null)
        {
            Debug.LogError("[AnimatedCharacterController] TABLE anchor not found!");
            return;
        }

        // TigerMover 체크
        var tigerMover = currentCharacter.GetComponent<TigerMover>();
        if (tigerMover != null)
        {
            // Tiger 오프셋 적용
            PlaceCharacterBehindScreenWithOffset(currentCharacter, tigerOffset);
            FadeUtility.Instance?.FadeIn(currentCharacter, fadeDuration, 1f);

            tigerMover.OnLastJumpFinished += TryStartDecalProjection;
            tigerMover.StartMoving(tableAnchor);

            Debug.Log("[AnimatedCharacterController] TigerMover started");
            return;
        }
        // fallback (혹시 mover 없는 prefab일 경우)
        PlaceCharacterBehindScreen(currentCharacter);
        FadeUtility.Instance?.FadeIn(currentCharacter, fadeDuration, 1f);
    }

    private void SpawnFlowerGroup(MRUKAnchor anchor)
    {
        currentScreenAnchor = anchor;

        MRUKAnchor tableAnchor = MRUKManager.Instance.TableAnchor;
        if (tableAnchor == null)
        {
            Debug.LogError("[AnimatedCharacterController] TABLE anchor not found!");
            return;
        }

        // flower offset 배열도 준비 가능
        Vector3[] offsets = new Vector3[]
        {
            butterflyOffsets.butterfly1Offset,
            butterflyOffsets.butterfly2Offset,
            butterflyOffsets.butterfly3Offset,
        };

        for (int i = 0; i < flowerPrefabs.Length; i++)
        {
            GameObject flower = Instantiate(flowerPrefabs[i]);
            
            // 위치 배치
            PlaceCharacterBehindScreenWithOffset(flower, offsets[i % offsets.Length]);

            // fade
            FadeUtility.Instance?.FadeIn(flower, fadeDuration, 1f);

            // mover 실행
            var mover = flower.GetComponent<ButterflyMover>();
            if (mover != null)
            {
                mover.StartMoving(tableAnchor, currentScreenAnchor);
            }

            Debug.Log($"Spawned flower #{i + 1}");
        }
    }



    private void SpawnPersonGroup(MRUKAnchor anchor)
    {
        if (personPrefabs == null || personPrefabs.Length != 3)
        {
            Debug.LogError("[AnimatedCharacterController] personPrefabs array must contain exactly 3 prefabs!");
            return;
        }

        currentScreenAnchor = anchor;

        // TABLE 앵커 가져오기
        MRUKAnchor tableAnchor = MRUKManager.Instance.TableAnchor;
        if (tableAnchor == null)
        {
            Debug.LogError("[AnimatedCharacterController] TABLE anchor missing!");
            return;
        }

        Vector3[] offsets = new Vector3[]
        {
            personOffsets.chunhyangOffset,
            personOffsets.sattoOffset,
            personOffsets.mongryongOffset
        };


        for (int i = 0; i < 3; i++)
        {
            // 각각 서로 다른 prefab을 instantiate
            GameObject person = Instantiate(personPrefabs[i]);

            // 위치 배치 (offset 적용)
            PlaceCharacterBehindScreenWithOffset(person, offsets[i]);

            // Fade-in
            FadeUtility.Instance?.FadeIn(person, fadeDuration, 1f);

            // 각 프리팹에 맞는 Mover 찾아서 실행
            bool moverFound = false;

            // ChunhyangMover 체크
            var chunhyangMover = person.GetComponent<ChunhyangMover>();
            if (chunhyangMover != null)
            {
                chunhyangMover.OnLastActionFinished += TryStartDecalProjection;
                chunhyangMover.StartMoving(tableAnchor, currentScreenAnchor);
                Debug.Log($"[AnimatedCharacterController] ChunhyangMover started for person #{i+1}");
                moverFound = true;
            }

            // SattoMover 체크
            if (!moverFound)
            {
                var sattoMover = person.GetComponent<SattoMover>();
                if (sattoMover != null)
                {
                    //sattoMover.OnLastActionFinished += TryStartDecalProjection;
                    sattoMover.StartMoving(tableAnchor, currentScreenAnchor);
                    Debug.Log($"[AnimatedCharacterController] SattoMover started for person #{i+1}");
                    moverFound = true;
                }
            }

            // MongryongMover 체크
            if (!moverFound)
            {
                var mongryongMover = person.GetComponent<MongryongMover>();
                if (mongryongMover != null)
                {
                    //mongryongMover.OnLastActionFinished += TryStartDecalProjection;
                    mongryongMover.StartMoving(tableAnchor, currentScreenAnchor);
                    Debug.Log($"[AnimatedCharacterController] MongryongMover started for person #{i+1}");
                    moverFound = true;
                }
            }

            if (!moverFound)
            {
                Debug.LogWarning($"[AnimatedCharacterController] No mover found for person #{i+1} ({personPrefabs[i].name})");
            }

            Debug.Log($"[AnimatedCharacterController] Spawned person #{i+1} ({personPrefabs[i].name})");
        }
    }


    private void PlaceCharacterBehindScreenWithOffset(GameObject obj, Vector3 extraOffset)
    {
        if (currentScreenAnchor == null)
            return;

        // 동일한 로직 적용
        MRUKAnchor tableAnchor = MRUKManager.Instance?.TableAnchor;
        if (tableAnchor == null)
        {
            Debug.LogError("[AnimatedCharacterController] TABLE anchor not found!");
            return;
        }

        Vector3 toTable = tableAnchor.transform.position - currentScreenAnchor.transform.position;
        float dot = Vector3.Dot(currentScreenAnchor.transform.up.normalized, toTable.normalized);
        
        Vector3 behindDirection;
        if (dot > 0)
        {
            behindDirection = -currentScreenAnchor.transform.up;
        }
        else
        {
            behindDirection = currentScreenAnchor.transform.up;
        }

        Vector3 pos = currentScreenAnchor.transform.position + (behindDirection * distanceFromScreen);

        // offset 적용 (Screen의 로컬 좌표계 기준)
        pos += currentScreenAnchor.transform.right * extraOffset.x;  // 좌우
        pos += behindDirection * extraOffset.z;  // 앞뒤 (배치 방향 기준)
        
        pos.y = 0f;

        obj.transform.position = pos;

        // 캐릭터가 Screen을 바라보도록
        Vector3 lookDir = -behindDirection;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            obj.transform.rotation = Quaternion.LookRotation(lookDir);
    }


    // Tiger 호출
    public void AppearTiger(MRUKAnchor anchor)
    {
        currentDecalType = DecalType.Tiger;
        SpawnTiger(tigerPrefab, anchor);
    }

    // Flower 호출
    public void AppearFlower(MRUKAnchor anchor)
    {
        currentDecalType = DecalType.Flower;
        SpawnFlowerGroup(anchor);
    }


    // Person 호출
    public void AppearPerson(MRUKAnchor anchor)
    {
        currentDecalType = DecalType.Person;
        SpawnPersonGroup(anchor);
    }

    // Tiger에서만 실행됨
    private void TryStartDecalProjection()
    {
        if (decalManager != null && currentFusedBaekja != null)
        {
            decalManager.StartDecal(currentFusedBaekja, currentDecalType);
        }
    }

    private void PlaceCharacterBehindScreen(GameObject obj)
    {
        if (currentScreenAnchor == null)
        {
            Debug.LogWarning("currentScreenAnchor missing");
            return;
        }

        // 테이블 위치 가져오기
        MRUKAnchor tableAnchor = MRUKManager.Instance?.TableAnchor;
        if (tableAnchor == null)
        {
            Debug.LogError("[AnimatedCharacterController] TABLE anchor not found!");
            return;
        }

        // Screen에서 Table로의 방향 벡터
        Vector3 toTable = tableAnchor.transform.position - currentScreenAnchor.transform.position;
        
        // Screen의 up 벡터와 Table 방향의 내적 계산
        float dot = Vector3.Dot(currentScreenAnchor.transform.up.normalized, toTable.normalized);
        
        // up이 Table을 향하고 있으면 반대쪽(-up)에, 아니면 같은쪽(+up)에 배치
        Vector3 behindDirection;
        if (dot > 0)
        {
            // Screen의 up이 Table을 향함 → 반대쪽에 배치
            behindDirection = -currentScreenAnchor.transform.up;
            Debug.Log($"[AnimatedCharacterController] Screen faces table → placing on opposite side (dot={dot:F2})");
        }
        else
        {
            // Screen의 up이 Table 반대편을 향함 → Table 쪽에 배치
            behindDirection = currentScreenAnchor.transform.up;
            Debug.Log($"[AnimatedCharacterController] Screen faces away from table → placing on table side (dot={dot:F2})");
        }
        
        // Screen 위치에서 배치 방향으로 distanceFromScreen만큼 이동
        Vector3 pos = currentScreenAnchor.transform.position + (behindDirection * distanceFromScreen);
        
        // Y축은 바닥에 고정
        pos.y = 0f;
        
        obj.transform.position = pos;
        
        // 캐릭터가 Screen을 바라보도록 회전
        Vector3 lookDirection = -behindDirection; // 배치 방향의 반대 = Screen 방향
        lookDirection.y = 0;
        
        if (lookDirection != Vector3.zero)
        {
            obj.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        Debug.Log($"[AnimatedCharacterController] Screen pos: {currentScreenAnchor.transform.position}, " +
                $"Placement direction: {behindDirection}, " +
                $"Character placed at: {pos}");
    }
}
