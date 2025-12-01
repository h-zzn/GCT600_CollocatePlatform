using UnityEngine;

public class DisplayManager : MonoBehaviour
{
    [Header("Canvas References")]
    public CanvasDisplayManager canvas1; // Canvas1 (호작도 - Tiger)
    public CanvasDisplayManager canvas2; // Canvas2 (평안감사도 - Person)
    public CanvasDisplayManager canvas3; // Canvas3 (초충도 - Flower)

    private void Update()
    {
        // 데모용 키 입력
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeImage("Tiger");
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeImage("Person");
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeImage("Flower");
    }

    public void ChangeImage(string message)
    {
        // [추가] 클라이언트 접속 메시지 처리
        if (message == "Client_Login")
        {
            Debug.Log("[알림] 새로운 클라이언트가 Display Scene에 입장했습니다!");
            return; // 그림 바꾸는 로직은 실행하지 않고 여기서 끝냄
        }

        Debug.Log($"[DisplayManager] ChangeImage received: '{message}'");
        Debug.Log($"[DisplayManager] === ChangeImage Called ===");
        Debug.Log($"[DisplayManager] Message: '{message}'");
        Debug.Log($"[DisplayManager] Message.Length: {message.Length}");
        Debug.Log($"[DisplayManager] Contains Tiger: {message.Contains("Tiger")}");
        Debug.Log($"[DisplayManager] Contains Person: {message.Contains("Person")}");
        Debug.Log($"[DisplayManager] Contains Flower: {message.Contains("Flower")}");
        Debug.Log($"[DisplayManager] ChangeImage received: '{message}'"); // 따옴표로 감싸서 확인

        // Contains를 쓰면 앞뒤에 공백이 있든, BOM이 있든, 깨진 문자가 있든
        // 핵심 단어만 들어있으면 작동합니다.
        
        if (message.Contains("Tiger")) 
        {
            if (canvas1 != null) canvas1.Toggle();
        }
        else if (message.Contains("Person")) 
        {
            if (canvas2 != null) canvas2.Toggle();
        }
        else if (message.Contains("Flower")) 
        {
            if (canvas3 != null) canvas3.Toggle();
        }
        else
        {
            Debug.LogWarning($"[DisplayManager] Unknown message: {message}");
        }
    }
}