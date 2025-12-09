using UnityEngine;

using System.Collections;



public class SimpleAligner : MonoBehaviour

{

    [Tooltip("유니티의 [BuildingBlock] Camera Rig를 여기에 넣으세요")]

    public Transform cameraRig;
    private bool isAligned = false;

    void Start()

    {

        // 게임 시작하자마자 앵커 찾기 시작!

        StartCoroutine(FindAndAlignRoutine());

    }



    IEnumerator FindAndAlignRoutine()

    {

        Debug.Log("[SimpleAligner] 앵커 수색을 시작합니다...");



        while (!isAligned)

        {

            // 1. 씬에 진짜 앵커(OVRSpatialAnchor)가 생겼는지 확인

            OVRSpatialAnchor realAnchor = FindObjectOfType<OVRSpatialAnchor>();



            // 앵커가 있고, 그 앵커가 '생성' 또는 '공유'되어 위치가 잡혔는지(Localized) 확인

            if (realAnchor != null && realAnchor.transform.position != Vector3.zero)

            {

                Debug.Log($"[SimpleAligner] 🎯 앵커 발견! ({realAnchor.name}) 위치: {realAnchor.transform.position}");

                AlignTo(realAnchor.transform);

                isAligned = true; // 정렬 끝! 루프 종료.

            }

            else

            {

                // 아직 없으면 1초 뒤에 다시 확인

                // Debug.Log("."); // 로그 너무 많이 뜨면 주석 처리

            }



            yield return new WaitForSeconds(1.0f); // 1초마다 체크

        }

    }



    void AlignTo(Transform anchorTransform)

    {

        if (cameraRig == null)

        {

            Debug.LogError("[SimpleAligner] ❌ Camera Rig가 연결되지 않았습니다!");

            return;

        }



        Debug.Log($"[SimpleAligner] 🚀 정렬 실행! (현재 리그 위치: {cameraRig.position})");



        // 카메라 리그를 앵커 기준점으로 이동

        float anchorY = anchorTransform.eulerAngles.y;

        cameraRig.position = cameraRig.position - anchorTransform.position;

        cameraRig.eulerAngles = new Vector3(0, cameraRig.eulerAngles.y - anchorY, 0);



        Debug.Log($"[SimpleAligner] ✨ 정렬 완료! 이제 위치가 맞아야 합니다.");

    }

   

    // 혹시 테스트 중 강제로 맞추고 싶으면 키보드 스페이스바를 누르세요 (에디터용)

    void Update()

    {

        if (Input.GetKeyDown(KeyCode.Space))

        {

            Debug.Log("[SimpleAligner] 강제 정렬 시도 (스페이스바)");

            isAligned = false; // 다시 찾게 만듦

            StartCoroutine(FindAndAlignRoutine());

        }

    }

}