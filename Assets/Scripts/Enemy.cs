﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI; // AI, 내비게이션 시스템 관련 코드를 가져오기

// 적 AI를 구현한다
public class Enemy : LivingEntity
{
    public LayerMask whatIsTarget; // 추적 대상 레이어

    private LivingEntity targetEntity; // 추적 대상
    private NavMeshAgent pathFinder; // 경로 계산 AI 에이전트

    public ParticleSystem hitEffect; // 피격 시 재생할 파티클 효과
    public AudioClip deathSound; // 사망 시 재생할 소리
    public AudioClip hitSound; // 피격 시 재생할 소리

    private Animator enemyAnimator; // 애니메이터 컴포넌트
    private AudioSource enemyAudioPlayer; // 오디오 소스 컴포넌트
    private Renderer enemyRenderer; // 렌더러 컴포넌트

    public float damage = 20; // 공격력
    public float timeBetAttack = 0.5f; // 공격 간격
    private float lastAttackTime; // 마지막 공격 시점

    private bool hasTarget
    {
        get
        {
            if(targetEntity != null && !targetEntity.dead) // 추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
            {
                return true;
            }

            return false; // 아니면 false;
        }       
    }

    private void Awake()
    {
        // 게임 오브젝트로부터 사용할 컴포넌트 가져오기
        pathFinder = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();
        enemyAudioPlayer = GetComponent<AudioSource>();

        enemyRenderer = GetComponentInChildren<Renderer>(); // 렌더러 컴포넌트는 자식 게임오브젝트에 있음
    }

    
    public void Setup(float newHealth, float newDamage, float newSpeed, Color skinColor) // 적 AI의 초기 스펙을 결정하는 셋업 메서드
    {
        startingHealth = newHealth; // 체력 설정
        health = newHealth;
        damage = newDamage; // 공격력 설정
        pathFinder.speed = newSpeed; // 내비메시 에이전트의 이동속도 설정
        enemyRenderer.material.color = skinColor; // 렌더러가 사용 중인 머티리얼의 컬러를 변경, 외형 색이 변함
    }

    private void Start()
    {
        StartCoroutine(UpdatePath()); // 게임 오브젝트 활성화와 동시에 AI의 추적 루틴 시작
    }

    private void Update()
    {
        enemyAnimator.SetBool("HasTarget", hasTarget); // 추적 대상의 존재 여부에 따라 다른 애니메이션을 재생
    }

    private IEnumerator UpdatePath()
    {
        while(!dead)
        {
            if(hasTarget) // 추적대상 존재 : 경로를 갱신하고 AI 이동을 계속 진행
            {
                pathFinder.isStopped = false;
                pathFinder.SetDestination(targetEntity.transform.position);
            }
            else
            {
                pathFinder.isStopped = true; // 추적대상 없음 : AI 이동 중지

                // 20유닛의 반지름을 가진 가상의 구를 그렸을 때 구와 겹치는 모든 콜라이더를 가져옴
                // 단, whatIsTarget 레이어를 가진 콜라이더만 가져오도록 필터링
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);

                for (int i = 0; i < colliders.Length; i++) // 모든 콜라이더를 순회하면서 살아 있는 LivingEntity 찾기
                {
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>(); // 콜라이더로부터 LivingEntity 컴포넌트 가져오기

                    if(livingEntity != null && !livingEntity.dead) // LivingEntity 컴포넌트가 존재하면, 해당 LivingEntity가 살아있다면
                    {
                        targetEntity = livingEntity; // 추적 대상을 해당 LivingEntity로 설정
                        break; // for문 루프 즉시 정지
                    }
                }
                
                
            }

            yield return new WaitForSeconds(0.25f); // 0.25초 주기로 처리 반복
        }
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {

        if (!dead) //  아직 사망하지 않은 경우에만 피격 효과 재생
        {
            // 공격받은 지점과 방향으로 파티클 효과 재생
            hitEffect.transform.position = hitPoint;
            hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            hitEffect.Play();

            enemyAudioPlayer.PlayOneShot(hitSound); // 피격 효과음 재생

        }

        base.OnDamage(damage, hitPoint, hitNormal); // LivingEntity의 OnDamage()를 실행하여 대미지 적용
    }

    public override void Die()
    {
        base.Die(); // LivingEntity의 Die()를 실행하여 대미지 적용

        // 다른 AI를 방해하지 않도록 자신의 모든 콜라이더 비활성화
        Collider[] enemyColliders = GetComponents<Collider>();
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            enemyColliders[i].enabled = false;
        }

        // AI 추적을 중지하고 내비메시 컴포넌트 비활성화
        pathFinder.isStopped = true;
        pathFinder.enabled = false;

        enemyAnimator.SetTrigger("Die"); // 사망 애니메이션 재생
        enemyAudioPlayer.PlayOneShot(deathSound); // 사망 효과음 재생
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!dead && Time.time >= lastAttackTime + timeBetAttack ) // 자신이 사망하지 않았으며, 최근 공격 시점에서 timeBetAttack 이상 시간이 지났다면 공격 가능
        {
            LivingEntity attackTarget = other.GetComponent<LivingEntity>(); // 상대방의 LivingEntity 타입 가져오기

            if(attackTarget != null && attackTarget == targetEntity) // 상대방의 LivingEntity가 자신의 추적 대상이라면 공격 실행
            {
                lastAttackTime = Time.time; // 최근 공격 시간 갱신

                // 상대방의 피격 위치와 피격 방향을 근사값으로 계산
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = transform.position - other.transform.position;

                attackTarget.OnDamage(damage, hitPoint, hitNormal); // 공격 실행
            }
        }




    }
}