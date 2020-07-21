using Photon.Pun;
using UnityEngine;

// 플레이어 캐릭터를 사용자 입력에 따라 움직이는 스크립트
public class PlayerMovement : MonoBehaviourPun {
    public float moveSpeed = 5f; // 앞뒤 움직임의 속도
    public float rotateSpeed = 180f; // 좌우 회전 속도


    private PlayerInput playerInput; // 플레이어 입력을 알려주는 컴포넌트
    private Rigidbody playerRigidbody; // 플레이어 캐릭터의 리지드바디
    private Animator playerAnimator; // 플레이어 캐릭터의 애니메이터

    private void Start() {
        // 사용할 컴포넌트들의 참조를 가져오기
        playerInput = GetComponent<PlayerInput>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
    }


    // FixedUpdate는 물리 갱신 주기에 맞춰 실행됨
    private void FixedUpdate()
    {
        if(!photonView.IsMine) // 로컬 플레이어만 직접 위치와 회전 변경 가능
        {
            return;
        }

        // 물리 갱신 주기마다 움직임, 회전, 애니메이션 처리 실행
        Rotate();
        Move();

        playerAnimator.SetFloat("Move", playerInput.move); // 입력값에 따라 애니메이터의 Move 파라미터값 변경
    }

    // 입력값에 따라 캐릭터를 앞뒤로 움직임
    private void Move() {
        Vector3 moveDistance =
            playerInput.move * transform.forward * moveSpeed * Time.deltaTime; // 상대적으로 이동할 거리 계산

        playerRigidbody.MovePosition(playerRigidbody.position + moveDistance); // 리지디바디를 이용해 게임 오브젝트 위치 변경(현재위치 +상대적으로 더 이동할 거리)
    }

    // 입력값에 따라 캐릭터를 좌우로 회전
    private void Rotate() {
        float turn = playerInput.rotate * rotateSpeed * Time.deltaTime; // 상대적으로 회전할 수치 계산

        playerRigidbody.rotation = playerRigidbody.rotation * Quaternion.Euler(0, turn, 0f); // 리지드바디를 이용해 게임 오브젝트 회전 변경
    }
}