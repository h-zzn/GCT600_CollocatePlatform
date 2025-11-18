using UnityEngine;
using Meta.XR.MRUtilityKit;

public class TigerMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform targetPoint;
    public float moveSpeed = 1.5f;
    public float stopDistance = 0.3f;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (targetPoint == null)
        {
            Debug.LogError("[TigerMover] Target Point is not assigned.");
        }
        
        if (MRUKManager.Instance != null)
        {
            if (MRUKManager.Instance.IsReady)
                AssignTableAnchor(MRUKManager.Instance.CurrentRoom);
            else
                MRUKManager.Instance.OnRoomReady += AssignTableAnchor;
        }
    }

    private void AssignTableAnchor(MRUKRoom room)
    {
        if (room == null)
        {
            Debug.LogWarning("[TigerMover] No MRUKRoom provided.");
            return;
        }

        foreach (var anchor in room.Anchors)
        {
            if (anchor.Label == MRUKAnchor.SceneLabels.TABLE)
            {
                targetPoint = anchor.transform;
                Debug.Log($"[TigerMover] TABLE Anchor set as target → {anchor.name}");
                return;
            }
        }

        Debug.LogWarning("[TigerMover] TABLE Anchor not found.");
    }

    private void Update()
    {
        if (targetPoint == null) return;
        
        if (!IsInWalkState()) return;

        MoveToTarget();
    }

    private bool IsInWalkState()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        return info.IsName("Walk");  
    }

    private void MoveToTarget()
    {
        Vector3 dir = targetPoint.position - transform.position;
        dir.y = 0;
        float dist = dir.magnitude;

        if (dist > stopDistance)
        {
            // 회전
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);

            // 이동
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else
        {
            // 도착 → Walk 끄기
            animator.SetBool("isWalking", false);
        }
    }
}
