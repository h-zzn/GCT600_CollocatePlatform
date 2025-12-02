using UnityEngine;

public class DecalManager : MonoBehaviour
{
    [Header("Decal Textures")]
    [SerializeField] private Texture2D tigerDecal;
    [SerializeField] private Texture2D flowerDecal;
    [SerializeField] private Texture2D personDecal;

    // 런타임에 자동으로 찾을 테이블용 Grass 페이더
    private GrassAutoFaderOnDecal tableGrassFader;

    // 기본값: Tiger
    public void StartDecal(GameObject baekja)
    {
        StartDecal(baekja, DecalType.Tiger);
    }

    public void StartDecal(GameObject baekja, DecalType type)
    {
        if (baekja == null)
        {
            Debug.LogWarning("[DecalManager] StartDecal: baekja == null");
            return;
        }

        // 1) 백자 안에서 ProjectionController 찾기
        ProjectionController proj =
            baekja.GetComponentInChildren<ProjectionController>(true);

        if (proj == null)
        {
            Debug.LogWarning("[DecalManager] ProjectionController not found on baekja.");
            return;
        }

        // 2) 텍스처 세팅 + 프로젝션
        Texture2D decalTex = GetTextureByType(type);
        if (decalTex == null)
        {
            Debug.LogWarning($"[DecalManager] No decal texture assigned for type: {type}");
        }

        proj.autoRun = false;

        if (decalTex != null)
            proj.SetDecalTexture(decalTex);

        proj.ProjectOnce();
        proj.PlayFadeIn();


        // 3) Tiger일 때만, 테이블 쪽 풀 페이드 시도
        if (type == DecalType.Tiger)
        {
            var grassFader = ResolveTableGrassFader();

            if (grassFader != null)
            {
                Debug.Log("[DecalManager] Tiger decal → Table GrassAutoFaderOnDecal.StartFade 호출");
                grassFader.StartFade();
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX3D(
                        SoundID.GrassGrow,
                        grassFader.transform.position
                    );
                }

            }
            else
            {
                Debug.LogWarning("[DecalManager] Tiger decal이지만 씬에서 GrassAutoFaderOnDecal을 찾지 못함");
            }
        }

        Debug.Log($"[DecalManager] StartDecal 완료 (type: {type}, tex: {(decalTex != null ? decalTex.name : "null")})");
    }

    /// <summary>
    /// 씬 안에서 테이블에 붙어 있는 GrassAutoFaderOnDecal을 1회 탐색 후 캐싱.
    /// (테이블이 하나라고 가정)
    /// </summary>
    private GrassAutoFaderOnDecal ResolveTableGrassFader()
    {
        if (tableGrassFader != null)
            return tableGrassFader;

        // 테이블 prefab이 이미 씬에 스폰된 이후라면 여기서 잡힌다.
        tableGrassFader = FindObjectOfType<GrassAutoFaderOnDecal>();

        if (tableGrassFader != null)
        {
            Debug.Log($"[DecalManager] GrassAutoFaderOnDecal 자동 연결 완료 (obj={tableGrassFader.gameObject.name})");
        }

        return tableGrassFader;
    }

    private Texture2D GetTextureByType(DecalType type)
    {
        switch (type)
        {
            case DecalType.Tiger:  return tigerDecal;
            case DecalType.Flower: return flowerDecal;
            case DecalType.Person: return personDecal;
            default:               return null;
        }
    }
}
