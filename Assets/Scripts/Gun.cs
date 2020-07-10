﻿using System.Collections;
using UnityEngine;

// 총을 구현한다
public class Gun : MonoBehaviour {
    // 총의 상태를 표현하는데 사용할 타입을 선언
    public enum State
    {
        Ready, // 발사 준비됨
        Empty, // 탄창이 빔
        Reloading // 재장전 중
    }

    public State state { get; private set; } // 현재 총의 상태

    public Transform fireTransform; // 탄알이 발사될 위치

    public ParticleSystem muzzleFlashEffect; // 총구 화염 효과
    public ParticleSystem shellEjectEffect; // 탄피 배출 효과

    private LineRenderer bulletLineRenderer; // 탄알 궤적을 그리기 위한 렌더러

    private AudioSource gunAudioPlayer; // 총 소리 재생기
    public AudioClip shotClip; // 발사 소리
    public AudioClip reloadClip; // 재장전 소리

    public float damage = 25; // 공격력
    private float fireDistance = 50f; // 사정거리

    public int ammoRemain = 100; // 남은 전체 탄알
    public int magCapacity = 25; // 탄창 용량
    public int magAmmo; // 현재 탄창에 남아 있는 탄알

    public float timeBetFire = 0.12f; // 탄알 발사 간격
    public float reloadTime = 1.8f; // 재장전 소요 시간
    private float lastFireTime; // 총을 마지막으로 발사한 시점(연사 구현시 사용)

    private void Awake() 
    {
        // 사용할 컴포넌트의 참조 가져오기
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();

        bulletLineRenderer.positionCount = 2; // 사용할 점을 두 개로 변경
        bulletLineRenderer.enabled = false; // 라인 렌더러 비활성화

    }

    private void OnEnable()
    {
        magAmmo = magCapacity; // 현재 탄창을 가득 채우기
        state = State.Ready; // 총의 현재 상태를 총을 쏠 준비가 된 상태로 변경
        lastFireTime = 0; // 마지막으로 총을 쏜 시점을 초기화
    }

    public void Fire()
    {
        if( state == State.Ready && Time.time >= lastFireTime + timeBetFire) // 현재상태가 발사 가능한 상태 && 마지막 총 발사 시점에서 발사간격 이상의 시간이 지남
        {
            lastFireTime = Time.time; // 마지막 총 발사 시점 갱신
            Shot(); // 실제 발사 처리 실행
        }
    }

    private void Shot()
    {
        RaycastHit hit; // 레이캐스트에 의한 충돌 정보를 저장하는 컨테이너

        Vector3 hitPosition = Vector3.zero; // 탄알이 맞은 곳을 저장할 변수

        if(Physics.Raycast(fireTransform.position, fireTransform.forward, out hit, fireDistance)) // 레이캐스트(시작점, 방향, 충돌 정보 컨테이너, 사정거리)
        {
            IDamageable target = hit.collider.GetComponentInParent<IDamageable>(); // 충돌한 상대방으로부터 IDamageable 오브젝트 가져오기

            if(target != null) // 상대방으로부터 IDamageable 오브젝트를 가져오는데 성공했다면
            {
                target.OnDamage(damage, hit.point, hit.normal); // 상대방의 OnDamage 함수를 실행시켜 상대방에 대미지 주기
            }

            hitPosition = hit.point; // 레이가 충돌한 위치 저장
        }
        else // 레이가 다른 물체와 충돌하지 않았다면
        {
            hitPosition = fireTransform.position + fireTransform.forward * fireDistance; // 탄알이 최대 사정거리까지 날아갔을 때의 위치를 충돌 위치로 사용
        }

        StartCoroutine(ShotEffect(hitPosition)); // hitPosition에 발사 이펙트 재생 시작

        magAmmo--; // 남은 탄알 수를 -1
        if(magAmmo <=0)
        {
            state = State.Empty; // 탄창에 남은 탄알이 없다면 총의 현재 상태를 Enpty로 갱신
        }

       
    }

    private IEnumerator ShotEffect(Vector3 hitPositon) // 발사 이펙트와 소리를 재생하고 탄알 궤적을 그림
    {
        muzzleFlashEffect.Play(); // 총구 화염 효과 재생
        shellEjectEffect.Play(); // 탄피 배출 효과 재생

        gunAudioPlayer.PlayOneShot(shotClip); // 총격 소리 재생

        bulletLineRenderer.SetPosition(0, fireTransform.position); // 선의 시작점은 총구의 위치
        bulletLineRenderer.SetPosition(1, hitPositon); // 선의 끝점은 입력으로 들어온 충돌 위치


        bulletLineRenderer.enabled = true; // 라인 렌더러를 활성화하여 탄알 궤적을 그림

        yield return new WaitForSeconds(0.03f); // 0.03초 동안 잠시 처리를 대기

        bulletLineRenderer.enabled = false; // 라인 렌더러를 비활성화하여 탄알 궤적을 지움
    }

    public bool Reload() // 재장전 시도
    {
        if(state == State.Reloading || ammoRemain <= 0 || magAmmo >= magCapacity) // 이미 재장전 중이거나, 남은 탄알이 없거나 탄창에 탄알이 이미 가득한 경우 재장전할 수 없음
        {
            return false;
        }

        StartCoroutine(ReloadRoutine()); // 재장전 처리
        return true;
        
    }

    private IEnumerator ReloadRoutine()
    {
        state = State.Reloading; // 현재 상태를 재장전 중 상태로 전환
        
        gunAudioPlayer.PlayOneShot(reloadClip); // 재장전 소리 재생

        yield return new WaitForSeconds(reloadTime); // 재장전 소요 시간만큼 처리 쉬기

        int ammoToFill = magCapacity - magAmmo; // 탄창에 채울 탄알을 계산

        if(ammoRemain < ammoToFill) // 탄창에 채워야할 탄알이 남은 탄알보다 많다면
        {
            ammoToFill = ammoRemain; // 채워야 할 탄알 수를 남은 탄알 수에 맞춰 줄임
        }

        magAmmo += ammoToFill; // 탄창을 채움
        ammoRemain -= ammoToFill; // 남은 탄알에서 탄창에 채운만큼 탄알을 뺌

        state = State.Ready; // 총의 현재 상태를 발사 준비 상태로 변경


        state = State.Ready; // 총의 현재 상태를 발사 준비된 상태로 변경
    }
}