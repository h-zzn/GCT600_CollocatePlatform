using UnityEngine;
using Meta.XR.MRUtilityKit;

public class AnimatedCharacterController : MonoBehaviour
{        
    [Header("Tiger Settings")]
    [SerializeField] private GameObject tigerObject;
    [SerializeField] private GameObject flowerObject;
    [SerializeField] private GameObject personObject;

    [SerializeField] private float distanceFromScreen = 2f;
    [SerializeField] private float fadeDuration = 2f;

    [SerializeField] private DecalManager decalManager;


    private Animator animator;
    private GameObject currentFusedBaekja; // fusedBaekja 참조 저장
    private MRUKAnchor currentScreenAnchor;


    private void Awake()
    {
        if (tigerObject == null)
            tigerObject = this.gameObject;

        animator = GetComponent<Animator>();

    }

    private void Start()
    {
        var mover = tigerObject.GetComponent<TigerMover>();
        decalManager = FindFirstObjectByType<DecalManager>();
        mover.OnLastJumpFinished += TryStartDecalProjection;

        // 초기에는 비활성 상태
        tigerObject.SetActive(false);
        
        // BaekjaHandler의 OnBaekjaCreated 이벤트 구독
        if (BaekjaHandler.Instance != null)
        {
            BaekjaHandler.Instance.OnBaekjaCreated += OnFusedBaekjaCreated;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (BaekjaHandler.Instance != null)
        {
            BaekjaHandler.Instance.OnBaekjaCreated -= OnFusedBaekjaCreated;
        }
    }

    // fusedBaekja가 생성되면 참조 저장
    private void OnFusedBaekjaCreated(GameObject fusedBaekja)
    {
        currentFusedBaekja = fusedBaekja;
        Debug.Log($"[TigerController] FusedBaekja 참조 저장: {fusedBaekja.name}");
    }

    public void AppearTiger(MRUKAnchor screenAnchor)
    {
        currentScreenAnchor = screenAnchor;

        PlaceCharacterBehindScreen(tigerObject);   // 해당 SCREEN 뒤에 배치

        tigerObject.SetActive(true);
        FadeUtility.Instance?.FadeIn(tigerObject, fadeDuration, 1f);
    }

    public void AppearFlower(MRUKAnchor screenAnchor)
    {
        currentScreenAnchor = screenAnchor;

        PlaceCharacterBehindScreen(flowerObject);   // 해당 SCREEN 뒤에 배치

        flowerObject.SetActive(true);
        FadeUtility.Instance?.FadeIn(flowerObject, fadeDuration, 1f);
    }

    public void AppearPerson(MRUKAnchor screenAnchor)
    {
        currentScreenAnchor = screenAnchor;

        PlaceCharacterBehindScreen(personObject);   // 해당 SCREEN 뒤에 배치

        personObject.SetActive(true);
        FadeUtility.Instance?.FadeIn(personObject, fadeDuration, 1f);
    }

    private void TryStartDecalProjection()
    {
        Debug.Log($"[TigerController] 데칼 시작 대상: {currentFusedBaekja.name}");
        decalManager.StartDecal(currentFusedBaekja);
    }

    private void PlaceCharacterBehindScreen(GameObject characterObject)
    {
        if (currentScreenAnchor == null)
        {
            Debug.LogWarning("[TigerController] currentScreenAnchor 없음");
            return;
        }
        
        Vector3 pos = currentScreenAnchor.transform.position;
        pos.x -= distanceFromScreen;
        pos.y = 0f;

        characterObject.transform.position = pos;

        // // 타이거가 스크린을 바라보게 회전
        // Quaternion rot = Quaternion.LookRotation(currentScreenAnchor.transform.position - pos);
        // tigerObject.transform.rotation = rot;

        Debug.Log("[TigerController] Tiger placed exactly at SCREEN anchor position.");
    }
}