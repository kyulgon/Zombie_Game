using UnityEngine;
using UnityEngine.AI; // 내비메쉬 관련 코드\
using System.Collections;
using Photon.Pun;

// 주기적으로 아이템을 플레이어 근처에 생성하는 스크립트
public class ItemSpawner : MonoBehaviour
{
    public GameObject[] items; // 생성할 아이템
    public Transform playerTransform; // 플레이어 트랜스폼

    public float maxDistance = 5f; // 플레이어 위치에서 아이템이 배치될 최대 반경

    public float timeBetSpawnMax = 7f; // 최대 시간 간격
    public float timeBetSpawnMin = 2f; // 최소 시간 간격
    private float timeBetSpawn; // 생성 간격

    private float lastSpawnTime; // 마지막 생성 시점

    private void Start()
    {
        // 생성 간격과 마지막 생성 시점 초기화
        timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
        lastSpawnTime = 0;
    }

    private void Update() // 주기적으로 아이템 생성 처리 실행
    {
        if(!PhotonNetwork.IsMasterClient) // 호스트에서만 아이템 직접 생성 가능
        {
            return;
        }

        // 현재시점이 마지막 생성 시점에서 생성 주기 이상 지남 && 플레이어 캐릭터가 존재 
        if (Time.time >= lastSpawnTime + timeBetSpawn && playerTransform != null)
        {
            lastSpawnTime = Time.time; // 마지막 생성 시간 갱신
            timeBetSpawn = Random.Range(timeBetSpawn, timeBetSpawnMax); // 생성 주기를 랜덤으로 변경
            Spawn(); // 아이템 생성 실행
        }
    }

    private void Spawn()
    {
        Vector3 spawnPosition = GetRandomPointOnMacMesh(Vector3.zero, maxDistance); // (0,0,0)을 기준으로 maxDistance 안에서 내비메시 위의 랜덤 위치 지정
        spawnPosition += Vector3.up * 0.5f; // 바닥에서 0.5만큼 위로 올리기

        GameObject selectedItem = items[Random.Range(0, items.Length)];  // 아이템 중 하나를 무작위로 골라 위치에 생성

        GameObject item = PhotonNetwork.Instantiate(selectedItem.name, spawnPosition, Quaternion.identity); // 네트워크의 모든 클라이언트에서 해당 아이템 생성

        StartCoroutine(DestroyAfter(item, 5f));  // 생성한 아이템을 5초 뒤에 파괴
    }

    IEnumerator DestroyAfter(GameObject target, float delay) // 포톤의 PhotonNetwork.Destroy()를 지연 실행하는 코루틴
    {
        yield return new WaitForSeconds(delay); // delay만큼 대기

        if(target != null) // target이 파괴되지 않았으면 파괴 실행
        {
            PhotonNetwork.Destroy(target);
        }
    }

    // 내비메시 위의 랜덤한 위치를 반환하는 메서드
    // cneter를 중심으로 distance 반경 안에서의 랜덤한 위치를 찾음
    private Vector3 GetRandomPointOnMacMesh(Vector3 center, float distance)
    {
        // center를 중심으로 반지름이 maxdistance인 구 안에서의 랜덤한 우치 하나를 저장
        // Random.insideUnitSphere는 반지름이 1인 구 안에서의 랜덤한 한 점을 반화하는 프로퍼티
        Vector3 randomPos = Random.insideUnitSphere * distance + center;

        NavMeshHit hit; // 내비메시 샘플링의 결과 정보를 저장하는 변수

        NavMesh.SamplePosition(randomPos, out hit, distance, NavMesh.AllAreas); // maxDistance 반경 안에서 randomPos에 가장 가까운 내비메시 위의 한 점을 찾음

        return hit.position; // 찾은 점 반환
    }



}