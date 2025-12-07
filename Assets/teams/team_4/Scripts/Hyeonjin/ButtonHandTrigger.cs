using UnityEngine;

public class ButtonHandTrigger : MonoBehaviour
{
    private ScreenLabelDisplay labelDisplay;
    private bool handWasInside = false;

    private void Start()
    {
        // 부모에서 ScreenLabelDisplay 찾기
        labelDisplay = GetComponentInParent<ScreenLabelDisplay>();
        
        if (labelDisplay == null)
        {
            Debug.LogError("[ButtonHandTrigger] ScreenLabelDisplay not found in parent!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // HandMarker가 아니면 무시
        if (other.GetComponent<HandMarker>() == null)
            return;

        // 손이 새로 "들어온 순간"만 버튼 클릭
        if (!handWasInside)
        {
            labelDisplay?.OnButtonTriggered();
            handWasInside = true;
            Debug.Log("[ButtonHandTrigger] Hand touched button!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 손이 나가면 리셋
        if (other.GetComponent<HandMarker>() != null)
        {
            handWasInside = false;
        }
    }
}