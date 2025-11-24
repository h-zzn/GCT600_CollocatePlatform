using UnityEngine;
using System.Collections;
using Meta.XR.MRUtilityKit;

public class JeongController : MonoBehaviour
{
    [SerializeField] private GameObject largeJeongPrefab;
    [SerializeField] private GameObject smallJeongPrefab;

    
    //[SerializeField] private float fadeDuration = 2f;    // 페이드 인/아웃 지속 시간
    [SerializeField] private float spawnYThreshold = 2.0f;

    [SerializeField] private TigerController tigerController;

    private void OnEnable()
    {
        if (BaekjaHandler.Instance != null)
            BaekjaHandler.Instance.OnBaekjaCreated += ShowJeongPrefab;
        else
            StartCoroutine(WaitAndSubscribe());

        JeongBehavior.OnJeongCollision += HandleJeongCollision;
    }

    private IEnumerator WaitAndSubscribe()
    {
        // Handler가 초기화될 때까지 대기
        while (BaekjaHandler.Instance == null)
            yield return null;

        BaekjaHandler.Instance.OnBaekjaCreated += ShowJeongPrefab;
    }

    private void OnDisable()
    {
        BaekjaHandler.Instance.OnBaekjaCreated -= ShowJeongPrefab;
        JeongBehavior.OnJeongCollision -= HandleJeongCollision;
    }

    private void ShowJeongPrefab(GameObject fusedBaekja)
    {
        Vector3  largeJeongPos = fusedBaekja.transform.position + Vector3.up * spawnYThreshold;
        GameObject largeJeongObj = Instantiate(largeJeongPrefab, largeJeongPos, Quaternion.identity);

        // 정 페이드인
        FadeUtility.Instance.FadeIn(largeJeongObj, 1f, 2f);
    }

    private void HandleJeongCollision(GameObject smallJeongObj, GameObject collidedObj)
    {
        Debug.Log($"Small Jeong collided with {collidedObj.name}");

        // 1. 충돌한 객체에서 MRUKAnchor 찾기
        MRUKAnchor hitAnchor =
            collidedObj.GetComponent<MRUKAnchor>() ??
            collidedObj.GetComponentInParent<MRUKAnchor>();

        if (hitAnchor == null) 
        {
            Debug.LogWarning("[JeongController] Collided object has no MRUKAnchor.");
            return;
        }

        // 2. SCREEN 앵커와 충돌했는지 확인
        if (hitAnchor.Label != MRUKAnchor.SceneLabels.SCREEN) 
        {
            Debug.Log("[JeongController] Jeong collided with non-SCREEN anchor. No action taken.");
            return;
        }

        // 3. SCREEN prefab의 SCREEN ID 확인
        ScreenIdentifier screenId = hitAnchor.GetComponentInChildren<ScreenIdentifier>();
        if (screenId == null) 
        {
            Debug.LogWarning("[JeongController] SCREEN anchor has no ScreenIdentifier component.");
            return;
        }

        Debug.Log($"[JeongController] Jeong collided with SCREEN ID: {screenId.screenID}");

        // 4. ID 기반으로 처리
        switch (screenId.screenID)
        {
            case "SCREEN_Tiger":
                Debug.Log("[JeongController] Activating Tiger for SCREEN_Tiger.");
            
                if (tigerController != null)
                {
                    tigerController.AppearTiger(hitAnchor);
                }
                else
                {
                    Debug.LogWarning("[JeongController] TigerController not assigned!");
                }
                break;

            // 다른 SCREEN ID 구현
            case "SCREEN_Flower":
                Debug.Log("[JeongController] Handling SCREEN_Flower collision.");   
                // Flower 관련 동작 구현
                break;

            case "SCREEN_Person":
                Debug.Log("[JeongController] Handling SCREEN_Person collision.");
                // Person 관련 동작 구현
                break;

            default:
                Debug.Log("[JeongController] No action defined for this SCREEN ID.");
                break;
        }

        FadeUtility.Instance.FadeOut(smallJeongObj, 1f);
        Destroy(smallJeongObj, 1.5f); // 페이드 아웃 후 제거
    }

    public void SpawnSmallJeong(Vector3 spawnPos)
    {
        GameObject small = Instantiate(smallJeongPrefab, spawnPos, Quaternion.identity);

        // // small Jeong은 기존 기능 그대로 (충돌 감지 등)
        // FadeUtility.Instance.FadeIn(small, 0.5f, 0f);
    }

    
}
