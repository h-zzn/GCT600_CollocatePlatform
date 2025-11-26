using UnityEngine;
using Meta.XR.MRUtilityKit;

public class ScreenAnchorDebugVisualizer : MonoBehaviour
{
    [Header("Gizmo Settings")]
    [SerializeField] private float arrowLength = 1f;
    [SerializeField] private Color forwardColor = Color.blue;
    [SerializeField] private Color rightColor = Color.red;
    [SerializeField] private Color upColor = Color.green;

    private void OnDrawGizmos()
    {
        // MRUKManager가 준비되지 않았으면 리턴
        if (MRUKManager.Instance == null || !MRUKManager.Instance.IsReady)
            return;

        var screenAnchors = MRUKManager.Instance.ScreenAnchors;
        
        if (screenAnchors == null || screenAnchors.Count == 0)
            return;

        foreach (var anchor in screenAnchors)
        {
            if (anchor == null) continue;

            Vector3 pos = anchor.transform.position;

            // Forward (파란색)
            Gizmos.color = forwardColor;
            DrawArrow(pos, anchor.transform.forward * arrowLength);

            // Right (빨간색)
            Gizmos.color = rightColor;
            DrawArrow(pos, anchor.transform.right * arrowLength);

            // Up (초록색)
            Gizmos.color = upColor;
            DrawArrow(pos, anchor.transform.up * arrowLength);

            // Screen 위치에 작은 구 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pos, 0.1f);
        }
    }

    private void DrawArrow(Vector3 pos, Vector3 direction)
    {
        // 화살표 몸통
        Gizmos.DrawRay(pos, direction);

        // 화살표 머리 (작은 콘 형태)
        Vector3 endPoint = pos + direction;
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * 0.1f;
        Vector3 up = Vector3.Cross(direction, right).normalized * 0.1f;

        Gizmos.DrawLine(endPoint, endPoint - direction.normalized * 0.2f + right);
        Gizmos.DrawLine(endPoint, endPoint - direction.normalized * 0.2f - right);
        Gizmos.DrawLine(endPoint, endPoint - direction.normalized * 0.2f + up);
        Gizmos.DrawLine(endPoint, endPoint - direction.normalized * 0.2f - up);
    }
}