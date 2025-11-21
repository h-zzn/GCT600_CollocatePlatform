using UnityEngine;

public class MarbleViewClipping : MonoBehaviour
{
    [Header("References")]
    public Transform hmd;
    public Transform bowl;
    public GameObject marble;
    private Material marbleMaterial;

    [Header("Bowl Settings")]
    public float bowlRadius = 0.15f;
    public float bowlDepth = 0.08f;
    public float bowlWallHeight = 0.03f;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public float manualClipOffset = 0f;

    private float marbleRadius;

    void Start()
    {
        Debug.Log("MarbleViewClipping Start!");

        Renderer renderer = marble.GetComponent<Renderer>();
        if (renderer != null)
        {
            marbleMaterial = renderer.material;
            Debug.Log("Material found: " + marbleMaterial.name);
        }
        else
        {
            Debug.LogError("Renderer not found on marble!");
        }

        if (hmd == null)
        {
            hmd = Camera.main.transform;
        }

        marbleRadius = marble.transform.localScale.y * 0.5f;

        Debug.Log($"Setup complete. Marble radius: {marbleRadius}");
    }

    void Update()
    {
        if (marbleMaterial == null || hmd == null || bowl == null || marble == null)
        {
            return;
        }

        float clipHeight = CalculateClipHeight();
        marbleMaterial.SetFloat("_ClipYPosition", clipHeight + manualClipOffset);
    }

    float CalculateClipHeight()
    {
        Vector3 cameraPos = hmd.position;
        Vector3 marbleCenter = marble.transform.position;
        Vector3 bowlCenter = bowl.position;
        float bowlRimY = bowlCenter.y + bowlWallHeight;

        // 카메라의 높이 (rim 기준)
        float cameraHeight = cameraPos.y - bowlRimY;

        // 카메라에서 구슬로의 방향
        Vector3 camToMarble = (marbleCenter - cameraPos).normalized;
        float verticalAngle = Mathf.Asin(-camToMarble.y) * Mathf.Rad2Deg;

        // === CASE 1: 카메라가 그릇보다 훨씬 위 ===
        if (cameraHeight > bowlDepth * 2)
        {
            // 거의 수직으로 내려다봄
            float viewFactor = Mathf.Clamp01(verticalAngle / 70f);
            return Mathf.Lerp(
                marbleCenter.y - marbleRadius * 0.9f,
                marbleCenter.y - marbleRadius * 0.2f,
                viewFactor
            );
        }

        // === CASE 2: 카메라가 그릇보다 훨씬 아래 ===
        if (cameraHeight < -bowlDepth)
        {
            // 아래에서 올려다봄 - 거의 다 가려짐
            return marbleCenter.y + marbleRadius * 0.9f;
        }

        // === CASE 3: 카메라가 그릇과 비슷한 높이 (옆에서 보기) ===
        return CalculateOcclusionFromSide(cameraPos, marbleCenter, bowlCenter, bowlRimY);
    }

    float CalculateOcclusionFromSide(Vector3 cameraPos, Vector3 marbleCenter, Vector3 bowlCenter, float bowlRimY)
    {
        // 수평 방향 계산
        Vector3 camToBowl2D = new Vector3(
            bowlCenter.x - cameraPos.x,
            0,
            bowlCenter.z - cameraPos.z
        );

        float horizontalDist = camToBowl2D.magnitude;

        if (horizontalDist < 0.001f)
        {
            // 카메라가 그릇 정중앙 위/아래
            return marbleCenter.y - marbleRadius * 0.5f;
        }

        Vector3 horizontalDir = camToBowl2D.normalized;

        // 카메라에서 가장 가까운 rim point
        Vector3 nearestRimPoint = new Vector3(
            bowlCenter.x - horizontalDir.x * bowlRadius,
            bowlRimY,
            bowlCenter.z - horizontalDir.z * bowlRadius
        );

        // === 핵심: 카메라가 rim보다 낮으면 rim이 구슬을 더 많이 가림 ===
        float heightDifference = cameraPos.y - bowlRimY;

        // 카메라가 rim과 거의 같은 높이거나 아래에 있으면
        if (heightDifference <= 0)
        {
            // Rim이 시야를 많이 막음
            // Rim의 윗면 기준으로 가림
            Vector3 rimToMarble = marbleCenter - nearestRimPoint;

            // Rim에서 구슬로 향하는 ray
            Vector3 rayDir = rimToMarble.normalized;

            // Ray-sphere intersection
            Vector3 oc = nearestRimPoint - marbleCenter;
            float a = 1.0f; // rayDir는 normalized
            float b = 2.0f * Vector3.Dot(oc, rayDir);
            float c = Vector3.Dot(oc, oc) - marbleRadius * marbleRadius;
            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                // Ray가 구슬을 맞추지 못함 - 완전히 가려짐
                return marbleCenter.y + marbleRadius;
            }

            float t = (-b - Mathf.Sqrt(discriminant)) / (2.0f * a);

            if (t < 0)
            {
                // 비정상 케이스
                return marbleCenter.y - marbleRadius;
            }

            Vector3 occlusionPoint = nearestRimPoint + t * rayDir;

            // 카메라가 낮을수록 더 많이 가려짐
            float heightFactor = Mathf.Clamp01(-heightDifference / bowlDepth);
            float additionalOcclusion = heightFactor * marbleRadius * 0.5f;

            return occlusionPoint.y + additionalOcclusion;
        }
        else
        {
            // 카메라가 rim보다 위 - 더 많이 보임
            Vector3 rimToMarble = marbleCenter - nearestRimPoint;
            Vector3 rayDir = rimToMarble.normalized;

            Vector3 oc = nearestRimPoint - marbleCenter;
            float a = 1.0f;
            float b = 2.0f * Vector3.Dot(oc, rayDir);
            float c = Vector3.Dot(oc, oc) - marbleRadius * marbleRadius;
            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                return marbleCenter.y + marbleRadius;
            }

            float t = (-b - Mathf.Sqrt(discriminant)) / (2.0f * a);
            if (t < 0) t = (-b + Mathf.Sqrt(discriminant)) / (2.0f * a);

            Vector3 occlusionPoint = nearestRimPoint + t * rayDir;

            // 위에서 볼수록 더 많이 보임
            float heightFactor = Mathf.Clamp01(heightDifference / (bowlDepth * 2));
            float lessOcclusion = heightFactor * marbleRadius * 0.4f;

            return occlusionPoint.y - lessOcclusion;
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos || bowl == null || marble == null) return;

        float bowlRimY = bowl.position.y + bowlWallHeight;
        float bowlBottomY = bowl.position.y - bowlDepth;

        // 그릇 rim (노란색)
        Gizmos.color = Color.yellow;
        DrawCircle(new Vector3(bowl.position.x, bowlRimY, bowl.position.z), bowlRadius, Vector3.up);

        // 그릇 바닥 (주황색)
        Gizmos.color = new Color(1f, 0.5f, 0f);
        DrawCircle(new Vector3(bowl.position.x, bowlBottomY, bowl.position.z), bowlRadius * 0.5f, Vector3.up);

        // 그릇 벽
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2 / 8;
            Vector3 rimEdge = bowl.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * bowlRadius;
            Gizmos.DrawLine(
                new Vector3(rimEdge.x, bowlRimY, rimEdge.z),
                new Vector3(rimEdge.x, bowlBottomY, rimEdge.z)
            );
        }

        if (hmd != null && Application.isPlaying)
        {
            // 카메라에서 구슬로 (파란색)
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(hmd.position, marble.transform.position);

            // 카메라에서 가장 가까운 rim point 계산
            Vector3 camToBowl2D = new Vector3(
                bowl.position.x - hmd.position.x,
                0,
                bowl.position.z - hmd.position.z
            );

            if (camToBowl2D.magnitude > 0.001f)
            {
                Vector3 horizontalDir = camToBowl2D.normalized;
                Vector3 nearestRimPoint = new Vector3(
                    bowl.position.x - horizontalDir.x * bowlRadius,
                    bowlRimY,
                    bowl.position.z - horizontalDir.z * bowlRadius
                );

                // Rim occlusion ray (cyan)
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(hmd.position, nearestRimPoint);
                Gizmos.DrawLine(nearestRimPoint, marble.transform.position);

                // Rim point 표시
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(nearestRimPoint, 0.01f);
            }

            // Clip plane (빨간색)
            if (marbleMaterial != null)
            {
                float clipHeight = CalculateClipHeight() + manualClipOffset;
                Gizmos.color = Color.red;
                Vector3 clipCenter = new Vector3(bowl.position.x, clipHeight, bowl.position.z);
                DrawCircle(clipCenter, bowlRadius * 1.2f, Vector3.up);

                // Clip height 수평선
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                Gizmos.DrawLine(
                    clipCenter + Vector3.left * bowlRadius,
                    clipCenter + Vector3.right * bowlRadius
                );
            }

            // 구슬 (초록색)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(marble.transform.position, marbleRadius);
        }
    }

    void DrawCircle(Vector3 center, float radius, Vector3 normal, int segments = 32)
    {
        Vector3 forward = Vector3.Slerp(Vector3.forward, Vector3.up, 0.5f);
        Vector3 right = Vector3.Cross(normal, forward).normalized;
        forward = Vector3.Cross(right, normal).normalized;

        Vector3 prevPoint = center + right * radius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            Vector3 newPoint = center + (right * Mathf.Cos(angle) + forward * Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}