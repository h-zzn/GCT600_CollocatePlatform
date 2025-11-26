using UnityEngine;
using Meta.XR.MRUtilityKit;
using System;

public class ChunhyangMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform targetPoint;
    public float firstWalkDistance = 3.0f;
    public float moveSpeed = 1.5f;
    public float stopDistance = 0.3f;
    public float fadeDuration = 2f;
    public float rotationSpeed = 5f;
    public float finalWalkDuration = 3f;

    private Animator animator;
    public event Action OnLastActionFinished;

    private Vector3 firstTargetPosition;
    private MRUKAnchor screenAnchor;
    
    private Transform mainCamera;
    private Quaternion savedRotation;

    private bool greetingCompleted = false;
    private float finalWalkStartTime = 0f;
    private bool rotationCallbackExecuted = false;

    private enum State
    {
        StandingIdle,
        FirstWalking,
        RotatingToCamera1,
        FirstGreeting,
        RotatingToTable,    // 새 상태: Table 방향으로 회전
        SecondWalking,
        RotatingToCamera2,
        FinalGreeting,
        RotatingBack2,      // 새 상태: 원래 방향으로 회전
        FinalWalking
    }

    private State currentState = State.StandingIdle;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main.transform;
    }

    public void StartMoving(MRUKAnchor tableAnchor, MRUKAnchor screenAnchor)
    {
        if (tableAnchor == null)
        {
            Debug.LogError("[ChunhyangMover] StartMoving: tableAnchor is NULL");
            return;
        }

        if (screenAnchor == null)
        {
            Debug.LogError("[ChunhyangMover] StartMoving: screenAnchor is NULL");
            return;
        }

        targetPoint = tableAnchor.transform;
        this.screenAnchor = screenAnchor;

        firstTargetPosition = screenAnchor.transform.position + 
                             (screenAnchor.transform.up * firstWalkDistance);
        firstTargetPosition.y = 0f;

        Debug.Log($"[ChunhyangMover] First target: {firstTargetPosition}");
        Debug.Log($"[ChunhyangMover] Final target (Table): {tableAnchor.name}");

        currentState = State.StandingIdle;
    }

    private void Update()
    {
        if (targetPoint == null) return;

        switch (currentState)
        {
            case State.StandingIdle:
                CheckStandingIdleToWalking();
                break;

            case State.FirstWalking:
                MoveToFirstTarget();
                break;

            case State.RotatingToCamera1:
                RotateToCamera(() => {
                    currentState = State.FirstGreeting;
                    greetingCompleted = false;
                    animator.SetTrigger("greet");
                });
                break;

            case State.FirstGreeting:
                CheckFirstGreetingEnd();
                break;

            // Table 방향으로 회전
            case State.RotatingToTable:
                RotateToTable(() => {
                    animator.SetBool("isWalking", true);
                    currentState = State.SecondWalking;
                    Debug.Log("[ChunhyangMover] Rotation to table complete → SecondWalking");
                });
                break;

            case State.SecondWalking:
                MoveToFinalTarget();
                break;

            case State.RotatingToCamera2:
                RotateToCamera(() => {
                    currentState = State.FinalGreeting;
                    greetingCompleted = false;
                    animator.SetTrigger("greet");
                });
                break;

            case State.FinalGreeting:
                CheckFinalGreetingEnd();
                break;

            // 원래 방향으로 회전
            case State.RotatingBack2:
                RotateBack(() => {
                    animator.SetBool("isWalking", true);
                    finalWalkStartTime = Time.time;
                    currentState = State.FinalWalking;
                    Debug.Log("[ChunhyangMover] Rotation back complete → FinalWalking (Fading out)");
                });
                break;

            case State.FinalWalking:
                ContinueFinalWalking();
                break;
        }
    }

    private void CheckStandingIdleToWalking()
    {
        if (IsInState("Standing Idle"))
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 0.95f)
            {
                animator.SetBool("isWalking", true);
                currentState = State.FirstWalking;
                Debug.Log("[ChunhyangMover] State: StandingIdle → FirstWalking");
            }
        }
    }

    private void MoveToFirstTarget()
    {
        if (!IsInState("Walking"))
            return;

        Vector3 dir = firstTargetPosition - transform.position;
        dir.y = 0;

        float dist = dir.magnitude;

        if (dist > stopDistance)
        {
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else
        {
            animator.SetBool("isWalking", false);
            savedRotation = transform.rotation;  // 현재 방향 저장
            rotationCallbackExecuted = false;
            currentState = State.RotatingToCamera1;
            Debug.Log("[ChunhyangMover] Reached first target → RotatingToCamera1");
        }
    }

    private void CheckFirstGreetingEnd()
    {
        if (IsInState("Standing Greeting"))
        {
            if (!greetingCompleted)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime >= 0.95f && !animator.IsInTransition(0))
                {
                    greetingCompleted = true;
                    rotationCallbackExecuted = false;
                    // ⭐ Table 방향으로 회전하도록 변경
                    currentState = State.RotatingToTable;
                    Debug.Log("[ChunhyangMover] FirstGreeting end → RotatingToTable");
                }
            }
        }
    }

    private void MoveToFinalTarget()
    {
        if (!IsInState("Walking 0"))
            return;

        Vector3 dir = targetPoint.position - transform.position;
        dir.y = 0;

        float dist = dir.magnitude;

        if (dist > stopDistance)
        {
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else
        {
            animator.SetBool("isWalking", false);
            savedRotation = transform.rotation;  // Table 도착 시 방향 저장
            rotationCallbackExecuted = false;
            currentState = State.RotatingToCamera2;
            Debug.Log("[ChunhyangMover] Reached Table → RotatingToCamera2");
        }
    }

    private void CheckFinalGreetingEnd()
    {
        if (IsInState("Standing Greeting 0"))
        {
            if (!greetingCompleted)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime >= 0.95f && !animator.IsInTransition(0))
                {
                    greetingCompleted = true;
                    rotationCallbackExecuted = false;
                    // ⭐ 원래 방향으로 회전하도록 변경
                    currentState = State.RotatingBack2;
                    Debug.Log("[ChunhyangMover] FinalGreeting end → RotatingBack2");
                }
            }
        }
    }

    private void ContinueFinalWalking()
    {
        if (!IsInState("Walking 1"))
            return;

        // 저장된 방향(savedRotation)으로 걷기
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        if (Time.time - finalWalkStartTime >= finalWalkDuration)
        {
            Debug.Log("[ChunhyangMover] Final walking complete!");
            OnLastActionFinished?.Invoke();
            FadeUtility.Instance?.FadeOutOpaque(gameObject, fadeDuration, 0f);
            Destroy(gameObject, fadeDuration);
        }
    }

    // Table 방향으로 회전하는 새 함수
    private void RotateToTable(System.Action onComplete)
    {
        Vector3 directionToTable = targetPoint.position - transform.position;
        directionToTable.y = 0;

        if (directionToTable == Vector3.zero)
        {
            if (!rotationCallbackExecuted)
            {
                rotationCallbackExecuted = true;
                onComplete?.Invoke();
            }
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionToTable);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 1f && !rotationCallbackExecuted)
        {
            transform.rotation = targetRotation;
            rotationCallbackExecuted = true;
            onComplete?.Invoke();
            Debug.Log("[ChunhyangMover] Rotation to table complete");
        }
    }

    private void RotateToCamera(System.Action onComplete)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("[ChunhyangMover] Main Camera not found!");
            if (!rotationCallbackExecuted)
            {
                rotationCallbackExecuted = true;
                onComplete?.Invoke();
            }
            return;
        }

        Vector3 directionToCamera = mainCamera.position - transform.position;
        directionToCamera.y = 0;

        if (directionToCamera == Vector3.zero)
        {
            if (!rotationCallbackExecuted)
            {
                rotationCallbackExecuted = true;
                onComplete?.Invoke();
            }
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 1f && !rotationCallbackExecuted)
        {
            transform.rotation = targetRotation;
            rotationCallbackExecuted = true;
            onComplete?.Invoke();
            Debug.Log("[ChunhyangMover] Rotation to camera complete");
        }
    }

    private void RotateBack(System.Action onComplete)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, savedRotation, Time.deltaTime * rotationSpeed);

        if (Quaternion.Angle(transform.rotation, savedRotation) < 1f && !rotationCallbackExecuted)
        {
            transform.rotation = savedRotation;
            rotationCallbackExecuted = true;
            onComplete?.Invoke();
            Debug.Log("[ChunhyangMover] Rotation back complete");
        }
    }

    private bool IsInState(string stateName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }
}