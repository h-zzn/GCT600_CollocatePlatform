using UnityEngine;

public class DecalManager : MonoBehaviour
{
    [Header("Decal Textures")]
    [SerializeField] private Texture2D tigerDecal;
    [SerializeField] private Texture2D flowerDecal;
    [SerializeField] private Texture2D personDecal;

    // 기존 함수와의 호환성 유지 (원하면 기본값을 tiger로)
    public void StartDecal(GameObject baekja)
    {
        StartDecal(baekja, DecalType.Tiger);
    }

    // 타입에 따라 텍스쳐 지정 
    public void StartDecal(GameObject baekja, DecalType type)
    {
        if (baekja == null)
        {
            Debug.LogWarning("[DecalManager] StartDecal: baekja == null");
            return;
        }

        ProjectionController proj = baekja.GetComponentInChildren<ProjectionController>(true);
        if (proj == null)
        {
            Debug.LogWarning("[DecalManager] StartDecal: ProjectionController not found on baekja.");
            return;
        }

        Texture2D decalTex = GetTextureByType(type);
        if (decalTex == null)
        {
            Debug.LogWarning($"[DecalManager] No decal texture assigned for type: {type}");
        }

        // 외부 제어 모드
        proj.autoRun = false;

        // 텍스처 지정
        if (decalTex != null)
            proj.SetDecalTexture(decalTex);

        proj.ProjectOnce();
        proj.PlayFadeIn();

        Debug.Log($"[DecalManager] StartDecal 완료 (type: {type}, tex: {(decalTex != null ? decalTex.name : "null")})");
    }

    private Texture2D GetTextureByType(DecalType type)
    {
        switch (type)
        {
            case DecalType.Tiger:
                return tigerDecal;
            case DecalType.Flower:
                return flowerDecal;
            case DecalType.Person:
                return personDecal;
            default:
                return null;
        }
    }
}
