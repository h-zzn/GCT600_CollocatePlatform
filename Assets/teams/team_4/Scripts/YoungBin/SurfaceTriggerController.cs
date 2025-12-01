using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SurfaceTriggerController : MonoBehaviour
{
    [Header("Common")]
    public string targetTag = "Marble";     // 구슬 태그
    public bool destroyOnHit = true;        // 닿으면 구슬 삭제
    public NetworkClient networkClient;     // UDP 송신(비워두면 자동탐색)

    private ScreenIdentifier screenIdentifier; // Screen ID 가져오기

    private void Awake()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;

        if (networkClient == null)
            networkClient = FindAnyObjectByType<NetworkClient>(); // 안전장치

        // Screen ID 컴포넌트 가져오기
        screenIdentifier = GetComponent<ScreenIdentifier>();
        
        if (screenIdentifier == null)
        {
            Debug.LogWarning($"[SurfaceTrigger] {gameObject.name}에 ScreenIdentifier가 없습니다!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;
        
        // 디버깅 추가
        Debug.Log($"[SurfaceTrigger] === TRIGGER ENTER ===");
        Debug.Log($"[SurfaceTrigger] GameObject: {gameObject.name}");
        if (screenIdentifier != null)
        {
            Debug.Log($"[SurfaceTrigger] screenID value: '{screenIdentifier.screenID}'");
            Debug.Log($"[SurfaceTrigger] screenID isEmpty: {string.IsNullOrEmpty(screenIdentifier.screenID)}");
        }
        else
        {
            Debug.LogError("[SurfaceTrigger] ScreenIdentifier is NULL!");
        }

        // 1) Screen ID 확인 후 메시지 전송
        if (networkClient != null && screenIdentifier != null)
        {
            string message = string.IsNullOrEmpty(screenIdentifier.screenID) 
                ? "Unknown" 
                : screenIdentifier.screenID;
            
            if (message == "Unknown")
            {
                Debug.LogError($"범인 발견! 이름: {gameObject.name}, ID: {gameObject.GetInstanceID()} 가 Unknown을 보냄!");
            }
            
            Debug.Log($"[SurfaceTrigger] Screen: {screenIdentifier.screenID} → Message: {message}");
            networkClient.SendData(message);
        }
        else if (networkClient == null)
        {
            Debug.LogWarning("[SurfaceTrigger] NetworkClient가 없습니다!");
        }
        else if (screenIdentifier == null)
        {
            Debug.LogWarning("[SurfaceTrigger] ScreenIdentifier가 없습니다!");
        }

        // 2) 구슬 제거
        if (destroyOnHit)
            Destroy(other.gameObject);
    }
}