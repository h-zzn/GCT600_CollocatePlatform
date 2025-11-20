using UnityEngine;
using System.Collections;

public class ProjectionController : MonoBehaviour
{
    [Header("Required")]
    public Shader shader;
    public Texture2D decalTex;
    public Transform projector;

    [Header("Auto-Run")]
    public bool autoRun = false;
    public bool fadeOnStart = true;
    public bool fallbackToMainCamera = true;

    [Header("Fade Settings")]
    public float fadeInDuration = 0.8f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Renderer targetRenderer;
    private Coroutine fadeRoutine;
    private int decalMatIndex = -1;

    void Start()
    {
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            Debug.LogWarning("[ProjectionController] Renderer 없음.");
            enabled = false; return;
        }

        if (shader == null)
        {
            Debug.LogWarning("[ProjectionController] Shader 미지정.");
            enabled = false; return;
        }

        if (projector == null && fallbackToMainCamera && Camera.main != null)
            projector = Camera.main.transform;

        // 기존 머티리얼 복사
        var mats = targetRenderer.materials; // 인스턴스 배열
        var newMats = new Material[mats.Length + 1];
        for (int i = 0; i < mats.Length; i++) newMats[i] = mats[i];

        // 새 데칼 머티리얼 생성
        var decalMat = new Material(shader);

        // 원본 정보 복사
        Material originalMat = targetRenderer.sharedMaterial;
        Texture baseMap = null;
        Color baseColor = Color.white;
        if (originalMat != null)
        {
            if (originalMat.HasProperty("_BaseMap"))
                baseMap = originalMat.GetTexture("_BaseMap");
            else if (originalMat.HasProperty("_MainTex"))
                baseMap = originalMat.GetTexture("_MainTex");

            if (originalMat.HasProperty("_BaseColor"))
                baseColor = originalMat.GetColor("_BaseColor");
            else if (originalMat.HasProperty("_Color"))
                baseColor = originalMat.GetColor("_Color");
        }

        if (baseMap != null) decalMat.SetTexture("_BaseMap", baseMap);
        decalMat.SetColor("_BaseColor", baseColor);
        if (decalTex != null) decalMat.SetTexture("_DecalTex", decalTex);

        if (decalMat.HasProperty("_DecalAlpha"))
            decalMat.SetFloat("_DecalAlpha", fadeOnStart ? 0f : 1f);

        // 배열 마지막에 붙이기
        decalMatIndex = newMats.Length - 1;
        newMats[decalMatIndex] = decalMat;
        targetRenderer.materials = newMats;

        if (autoRun)
            StartCoroutine(IE_AutoProjectAndShow());
    }

    private Material GetDecalMat()
    {
        if (targetRenderer == null) return null;
        if (decalMatIndex < 0) return null;

        var mats = targetRenderer.materials; // 현재 인스턴스 배열
        if (decalMatIndex >= mats.Length) return null;

        return mats[decalMatIndex];
    }

    private IEnumerator IE_AutoProjectAndShow()
    {
        yield return null;

        ProjectOnce();

        if (fadeOnStart) PlayFadeIn(fadeInDuration);
        else ShowInstant();
    }

    void Update()
    {
        var decalMat = GetDecalMat();
        if (projector == null || decalMat == null) return;

        Matrix4x4 view = projector.worldToLocalMatrix;
        Matrix4x4 proj = Matrix4x4.Ortho(-1, 1, -1, 1, 0.01f, 10f);

        Matrix4x4 uv = Matrix4x4.identity;
        uv.m00 = 0.5f; uv.m03 = 0.5f;
        uv.m11 = 0.5f; uv.m13 = 0.5f;

        Matrix4x4 projectorMatrix = uv * proj * view;
        decalMat.SetMatrix("_ProjectorMatrix", projectorMatrix);
    }

    public void ProjectOnce(float near = 0.01f, float far = 10f)
    {
        var decalMat = GetDecalMat();
        if (decalMat == null)
        {
            Debug.LogWarning("[ProjectionController] ProjectOnce: decalMat == null");
            return;
        }
        if (projector == null)
        {
            Debug.LogWarning("[ProjectionController] ProjectOnce: projector == null");
            return;
        }

        Matrix4x4 view = projector.worldToLocalMatrix;
        Matrix4x4 proj = Matrix4x4.Ortho(-1, 1, -1, 1, near, far);

        Matrix4x4 uv = Matrix4x4.identity;
        uv.m00 = 0.5f; uv.m03 = 0.5f;
        uv.m11 = 0.5f; uv.m13 = 0.5f;

        Matrix4x4 projectorMatrix = uv * proj * view;
        decalMat.SetMatrix("_ProjectorMatrix", projectorMatrix);

        Debug.Log("[ProjectionController] ProjectOnce 완료");
    }

    public void ShowInstant()
    {
        var decalMat = GetDecalMat();
        if (decalMat == null)
        {
            Debug.LogWarning("[ProjectionController] ShowInstant: decalMat == null");
            return;
        }

        float before = decalMat.HasProperty("_DecalAlpha") ? decalMat.GetFloat("_DecalAlpha") : -1f;
        decalMat.SetFloat("_DecalAlpha", 1f);
        float after = decalMat.HasProperty("_DecalAlpha") ? decalMat.GetFloat("_DecalAlpha") : -1f;

        Debug.Log($"[ProjectionController] ShowInstant");
    }

    public void HideInstant()
    {
        var decalMat = GetDecalMat();
        if (decalMat == null) return;
        if (!decalMat.HasProperty("_DecalAlpha")) return;

        decalMat.SetFloat("_DecalAlpha", 0f);
    }

    public void PlayFadeIn(float? durationOverride = null)
    {
        var decalMat = GetDecalMat();
        if (decalMat == null)
        {
            Debug.LogWarning("[ProjectionController] PlayFadeIn: decalMat == null");
            return;
        }

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(1f, durationOverride ?? fadeInDuration));
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        var decalMat = GetDecalMat();
        if (decalMat == null) yield break;
        if (!decalMat.HasProperty("_DecalAlpha")) yield break;

        float start = decalMat.GetFloat("_DecalAlpha");
        float t = 0f;

        while (t < 1f)
        {
            t += (duration > 0f ? Time.deltaTime / duration : 1f);
            float eased = fadeCurve.Evaluate(Mathf.Clamp01(t));
            float value = Mathf.LerpUnclamped(start, target, eased);
            decalMat.SetFloat("_DecalAlpha", value);
            yield return null;
        }

        decalMat.SetFloat("_DecalAlpha", target);
        fadeRoutine = null;

        Debug.Log("[ProjectionController] FadeTo 종료");
    }
}
