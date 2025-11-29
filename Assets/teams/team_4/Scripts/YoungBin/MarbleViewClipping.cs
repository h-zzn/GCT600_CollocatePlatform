using UnityEngine;

public class MarbleViewClipping : MonoBehaviour
{
    [Header("References")]
    public Transform hmd;
    public Transform bowl;
    public GameObject marble;
    private Material marbleMaterial;

    // Bowl settings - BowlController가 자동으로 설정
    private float bowlRadius;
    private float bowlDepth;
    private float bowlWallHeight;

    [Header("Debug")]
    public bool showDebugGizmos = true;

    private float marbleRadius;

    // 이 메서드 추가!
    public void SetBowlDimensions(float radius, float depth, float wallHeight)
    {
        bowlRadius = radius;
        bowlDepth = depth;
        bowlWallHeight = wallHeight;
        Debug.Log($"MarbleViewClipping: Bowl dimensions set - radius={radius}, depth={depth}, wallHeight={wallHeight}");
    }

    void Start()
    {
        Debug.Log("MarbleViewClipping Start!");

        Renderer renderer = marble.GetComponent<Renderer>();
        if (renderer != null)
        {
            marbleMaterial = renderer.material;
            Debug.Log("Material found: " + marbleMaterial.name);
            Debug.Log("Material Shader: " + marbleMaterial.shader.name);
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
        if (marbleMaterial == null || bowl == null || marble == null)
        {
            Debug.LogWarning("MarbleViewClipping: Missing references!");
            return;
        }

        marbleMaterial.SetVector("_BowlCenter", bowl.position);
        marbleMaterial.SetVector("_BowlUp", bowl.up);
        marbleMaterial.SetFloat("_BowlRadius", bowlRadius);
        marbleMaterial.SetFloat("_BowlDepth", bowlDepth);
        marbleMaterial.SetFloat("_BowlWallHeight", bowlWallHeight);
        marbleMaterial.SetFloat("_MarbleRadius", marbleRadius);
        
        if (Time.frameCount == 100)
        {
            Debug.Log($"Shader values - BowlCenter: {bowl.position}, BowlUp: {bowl.up}, Radius: {bowlRadius}, MarbleRadius: {marbleRadius}");
            Debug.Log($"Material Shader: {marbleMaterial.shader.name}");
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos || bowl == null || marble == null) return;

        float bowlBottomY = bowl.position.y;  // pivot이 바닥
        float bowlRimY = bowl.position.y + bowlDepth;  // rim이 위

        Gizmos.color = Color.yellow;
        DrawCircle(new Vector3(bowl.position.x, bowlRimY, bowl.position.z), bowlRadius, Vector3.up);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        DrawCircle(new Vector3(bowl.position.x, bowlBottomY, bowl.position.z), bowlRadius * 0.5f, Vector3.up);

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
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(hmd.position, marble.transform.position);

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

                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(hmd.position, nearestRimPoint);
                Gizmos.DrawLine(nearestRimPoint, marble.transform.position);

                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(nearestRimPoint, 0.01f);
            }

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