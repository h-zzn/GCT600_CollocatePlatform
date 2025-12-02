using System.Collections;
using UnityEngine;
using MicahW.PointGrass;

[DisallowMultipleComponent]
public class GrassAutoFaderOnDecal : MonoBehaviour
{
    [Header("Fade Parameters")]
    public float fadeDuration = 2f;

    private PointGrassRenderer[] renderers;

    private void Awake()
    {
        renderers = GetComponentsInChildren<PointGrassRenderer>(includeInactive: true);

        int count = renderers != null ? renderers.Length : 0;
        Debug.Log($"[GrassAutoFader] Awake - PointGrassRenderer {count}개 발견 (root={name})");

        if (renderers == null) return;

        // 시작 시에는 전부 0으로 (가능한 한 안 보이게)
        foreach (var r in renderers)
        {
            if (r == null) continue;
            r.SetPointLODFactor(0f);
        }
    }

    public void StartFade()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<PointGrassRenderer>(includeInactive: true);
            int count = renderers != null ? renderers.Length : 0;
            Debug.Log($"[GrassAutoFader] StartFade - 재검색 결과 {count}개");
            if (count == 0) return;
        }

        foreach (var r in renderers)
        {
            if (r == null) continue;
            r.SetPointLODFactor(0f);
        }

        StopAllCoroutines();
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        if (renderers == null || renderers.Length == 0)
            yield break;

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float v = Mathf.Clamp01(t / fadeDuration);

            foreach (var r in renderers)
            {
                if (r == null) continue;
                r.SetPointLODFactor(v);
            }

            yield return null;
        }

        foreach (var r in renderers)
        {
            if (r == null) continue;
            r.SetPointLODFactor(1f);
        }

        Debug.Log("[GrassAutoFader] FadeRoutine 완료 - LODFactor=1");
    }
}
