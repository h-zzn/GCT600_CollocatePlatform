using UnityEngine;
using System.Collections;
using Oculus.Interaction.HandGrab; 
using Oculus.Interaction;          

public class MarbleSpawnOnTouch : MonoBehaviour
{
    [Header("Marble Prefab")]
    [SerializeField] private GameObject independentMarblePrefab;
    
    private bool hasBeenTouched = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasBeenTouched) return;
        
        // 유효한 터치/그랩 시도 감지
        if (other.name.Contains("Controller") || 
            other.name.Contains("Grab") ||
            other.name.Contains("Pinch"))
        {
            hasBeenTouched = true;
            SpawnIndependentMarble(); 
        }
    }

    void SpawnIndependentMarble()
    {
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;
        
        // 새 Marble 생성
        if (independentMarblePrefab != null)
        {
            GameObject newMarble = Instantiate(independentMarblePrefab, currentPosition, currentRotation);
            newMarble.transform.SetParent(null);
            newMarble.name = "Marble_Independent";
        }
        else
        {
            Debug.LogError("Independent Marble Prefab is NOT assigned!");
        }
        
        // Marble 오브젝트를 파괴 대신 비활성화 처리
        StartCoroutine(SafeDisableRoutine());
    }

    // 파괴 대신 비활성화(Invisible) 처리를 수행하는 코루틴
    IEnumerator SafeDisableRoutine()
    {
        // =======================================================
        // 1. MarbleViewClipping 참조 해제 및 비활성화
        // =======================================================
        Transform bowlTransform = transform.parent; 
        if (bowlTransform != null)
        {
            MarbleViewClipping clipping = bowlTransform.GetComponentInChildren<MarbleViewClipping>();
            if (clipping != null)
            {
                clipping.marble = null; 
                clipping.enabled = false; 
            }
        }
        
        // =======================================================
        // 2. 상호작용/물리 컴포넌트 정리
        // =======================================================
        var interactable = GetComponent<HandGrabInteractable>();
        var collider = GetComponent<Collider>();
        var renderer = GetComponent<Renderer>();
        var rb = GetComponent<Rigidbody>();
        
        if (interactable != null) interactable.enabled = false; 
        if (collider != null) collider.enabled = false; 
        if (renderer != null) renderer.enabled = false;
        
        if (rb != null)
        {
            rb.detectCollisions = false; 
            rb.isKinematic = true; 
        }

        // =======================================================
        // 3. 오브젝트 비활성화
        // =======================================================
        // 모든 컴포넌트 비활성화 후, 오브젝트 자체를 비활성화합니다.
        yield return new WaitForEndOfFrame(); 
        
        gameObject.SetActive(false);
        Debug.Log("Marble made invisible and disabled (Destroy avoided).");
        
        yield break;
    }
}