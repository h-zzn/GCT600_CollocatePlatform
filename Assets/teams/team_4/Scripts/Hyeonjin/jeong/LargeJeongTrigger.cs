using UnityEngine;

public class LargeJeongTrigger : MonoBehaviour
{
    private bool handWasInside = false;  // 손이 들어왔던 기록
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 손이 아니면 무시
        if (other.GetComponent<HandMarker>() == null)
            return;

        // 손이 새로 "들어온 순간"만 small Jeong 생성
        if (!handWasInside)
        {
            SpawnSmallJeong();
            handWasInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 손 Collider가 나가면 리셋
        if (other.GetComponent<HandMarker>() != null)
        {
            handWasInside = false;
        }
    }

    private void SpawnSmallJeong()
    {
        Debug.Log("Small Jeong Spawned (trigger event).");

        // 작은 정 생성
        FindObjectOfType<JeongController>()?.SpawnSmallJeong(transform.position);
        
        // 사운드 재생
        if (SoundManager.Instance != null)
        SoundManager.Instance.PlaySFX3D(SoundID.Jeong, transform.position);
        // 큰 정 애니메이션 재생 (Trigger 파라미터)
        animator.SetTrigger("isTouched");

    }
}
