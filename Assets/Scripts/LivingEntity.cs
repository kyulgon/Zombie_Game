using Photon.Pun;
using System;
using UnityEngine;

// 생명체로서 동작할 게임 오브젝트들을 위한 뼈대를 제공
// 체력, 데미지 받아들이기, 사망 기능, 사망 이벤트를 제공
public class LivingEntity : MonoBehaviourPun, IDamageable
{
    public float startingHealth = 100f; // 시작 체력
    public float health { get; protected set; } // 현재 체력
    public bool dead { get; protected set; } // 사망 상태
    public event Action onDeath; // 사망 시 발동할 이벤트

    [PunRPC]
    public void ApplyUpdatedHealth(float newHealth, bool newDead) // 호스트 -> 모든 클라이언트 방향으로 체력과 사망 상태를 동기화하는 메서드
    {
        health = newHealth;
        dead = newDead;
    }

    protected virtual void OnEnable() // 생명체가 활성화될 때 상태를 리셋
    {
        dead = false; // 사망하지 않은 상태로 시작
        health = startingHealth; // 체력을 시작 체력으로 초기화
    }

    // 대미지 처리
    [PunRPC] // 호스트에서 먼저 단독 실행되고, 호스트를 통해 다른 클라이언트에서 일관 실행됨
    public virtual void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal) // 대미지를 입는 기능
    {
        if(PhotonNetwork.IsMasterClient)
        {
            health -= damage; // 대미지만큼 체력 감소

            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, health, dead); // 호스트에서 클라이언트로 동기화

            photonView.RPC("OnDamage", RpcTarget.Others, damage, hitPoint, hitNormal); // 다른 클라이언트도 OnDamage를 실행하도록 함
        }           

        if(health <= 0 && !dead) // 체력이 0이하거나 아직 죽지 않았다면 사망 처리 실행
        {
            Die();
        }
    }

    public virtual void RestoreHealth(float newHealth)
    {
        if(dead)
        {
            // 이미 사망한 경우 체력을 회복할 수 없음
            return;
        }

        if(PhotonNetwork.IsMasterClient)
        {
            health += newHealth; //  체력 추가

            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, health, dead); // 서버에서 클라이언트로 동기화

            photonView.RPC("RestoreHealth", RpcTarget.Others, newHealth); // 다른 클라이언트도 RestoreHealth를 실행하도록 함

        }

       
    }

    public virtual void Die()
    {
        if(onDeath != null) //  onDeath 이벤트에 등록된 메서드가 있다면 실행
        {
            onDeath();
        }

        dead = true; // 사망 상태를 참으로 변경
    }




}