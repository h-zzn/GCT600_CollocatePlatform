using UnityEngine;

public class BowlDummy : MonoBehaviour
{
    [Header("Bowl Dimensions")]
    public float radius = 0.15f;
    public float depth = 0.08f;
    public int segments = 32;
    
    [Header("Grabbable Settings")]
    public bool makeGrabbable = true;
    public bool useGravity = false;  // 중력 사용 여부

    void Start()
    {
        CreateBowlMesh();
    }

    void CreateBowlMesh()
    {
        GameObject bowlObj = new GameObject("BowlMesh");
        bowlObj.transform.parent = transform;
        bowlObj.transform.localPosition = Vector3.zero;

        MeshFilter meshFilter = bowlObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = bowlObj.AddComponent<MeshRenderer>();

        meshFilter.mesh = GenerateBowlMesh();

        // Material
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = new Color(0.8f, 0.8f, 0.8f, 0.3f);
        meshRenderer.material = mat;

        // Grabbable을 위한 Physics 컴포넌트
        if (makeGrabbable)
        {
            // Collider (잡기 위해 필요)
            MeshCollider collider = bowlObj.AddComponent<MeshCollider>();
            collider.convex = true;  // Grabbable/Rigidbody에 필수
            
            // Rigidbody (물리 시뮬레이션)
            Rigidbody rb = bowlObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;  // 기본: 완전 고정 (위치 안 변함)
            rb.useGravity = useGravity;  // Inspector에서 설정 가능
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            // 회전 가능하도록 FreezeRotation 제거!
            
            Debug.Log("Bowl mesh created with Grabbable support (Rigidbody + Collider, rotation enabled)");
        }
        else
        {
            Debug.Log("Bowl mesh created (static)");
        }
    }

    Mesh GenerateBowlMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            vertices[i] = new Vector3(x, 0, z);
            vertices[i + segments + 1] = new Vector3(x * 0.5f, -depth, z * 0.5f);
        }

        for (int i = 0; i < segments; i++)
        {
            int ti = i * 6;
            triangles[ti] = i;
            triangles[ti + 1] = i + segments + 1;
            triangles[ti + 2] = i + 1;

            triangles[ti + 3] = i + 1;
            triangles[ti + 4] = i + segments + 1;
            triangles[ti + 5] = i + segments + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}