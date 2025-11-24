using UnityEngine;
using Meta.XR.MRUtilityKit;

public class TigerController : MonoBehaviour
{        
    [Header("Tiger Settings")]
    [SerializeField] private GameObject tigerObject;
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

        PlaceTigerBehindScreen();   // 해당 SCREEN 뒤에 배치

        tigerObject.SetActive(true);
        FadeUtility.Instance?.FadeIn(tigerObject, fadeDuration, 1f);
    }

    private void TryStartDecalProjection()
    {
        Debug.Log($"[TigerController] 데칼 시작 대상: {currentFusedBaekja.name}");
        decalManager.StartDecal(currentFusedBaekja);
    }

    private void PlaceTigerBehindScreen()
    {
        if (currentScreenAnchor == null)
        {
            Debug.LogWarning("[TigerController] currentScreenAnchor 없음");
            return;
        }
        
        Vector3 pos = currentScreenAnchor.transform.position;
        pos.x -= distanceFromScreen;
        pos.y = 0f;

        tigerObject.transform.position = pos;

        // // 타이거가 스크린을 바라보게 회전
        // Quaternion rot = Quaternion.LookRotation(currentScreenAnchor.transform.position - pos);
        // tigerObject.transform.rotation = rot;

        Debug.Log("[TigerController] Tiger placed exactly at SCREEN anchor position.");
    }
}