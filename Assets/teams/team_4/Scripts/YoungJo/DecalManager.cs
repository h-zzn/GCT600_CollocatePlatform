using UnityEngine;

public class DecalManager : MonoBehaviour
{
    public void StartDecal(GameObject baekja)
    {

        ProjectionController proj = null;

        proj = baekja.GetComponentInChildren<ProjectionController>(true);

        // 외부 제어 모드
        proj.autoRun = false;
        proj.ProjectOnce();
        proj.PlayFadeIn();

        Debug.Log($"[DecalManager] '{proj.gameObject.name}' 호출 완료");
    }
}
