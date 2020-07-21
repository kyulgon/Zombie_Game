﻿using Photon.Pun;
using UnityEngine;

// 주어진 Gun 오브젝트를 쏘거나 재장전
// 알맞은 애니메이션을 재생하고 IK를 사용해 캐릭터 양손이 총에 위치하도록 조정
public class PlayerShooter : MonoBehaviourPun
{
    public Gun gun; // 사용할 총
    public Transform gunPivot; // 총 배치의 기준점
    public Transform leftHandMount; // 총의 왼쪽 손잡이, 왼손이 위치할 지점
    public Transform rightHandMount; // 총의 오른쪽 손잡이, 오른손이 위치할 지점

    private PlayerInput playerInput; // 플레이어의 입력
    private Animator playerAnimator; // 애니메이터 컴포넌트

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        gun.gameObject.SetActive(true); // 슈터가 활성화될 때 총도 함께 활성화
    }

    private void OnDisable()
    {
        gun.gameObject.SetActive(false); // 슈터가 비활성화될 때 총도 함께 비활성화
    }

    private void Update()
    {
        if(!photonView.IsMine) // 로컬 플레이어만 총을 직접 사격. 탄알 UI 갱신 가능
        {
            return;
        }

        if(playerInput.fire)
        {
            gun.Fire(); // 발사 입력 감지 시 총 발사
        }
        else if(playerInput.reload) // 재장전 입력 감지 시 재장전
        {
            if(gun.Reload())
            {
                playerAnimator.SetTrigger("Reload"); // 재장전 성공 시에만 재장전 애니메이션 재생
            }
        }

        UpdateUI(); // 남은 탄알 UI 갱신
    }

    private void UpdateUI() // 탄알 UI 갱신
    {
        if(gun != null && UIManager.instance != null)
        {
            UIManager.instance.UpdateAmmoText(gun.magAmmo, gun.ammoRemain); // UI 매니저의 탄알 텍스트에 탄창의 탄알과 남은 전체 탄알을 표시
        }

    }

    private void OnAnimatorIK(int layerIndex)
    {
        gunPivot.position = playerAnimator.GetIKHintPosition(AvatarIKHint.RightElbow); // 총의 기준점 gunPivot을 3D 모델의 오른쪽 팔꿈치 위치로 이동

        // IK를 사용하여 왼손의 위치와 회전을 총의 왼쪽 손잡이에 맞춤
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandMount.rotation);

        // IK를 사용하여 왼손의 위치와 회전을 총의 오른쪽 손잡이에 맞춤
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.RightHand, rightHandMount.rotation);


    }
}