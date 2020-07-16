using UnityEngine;
using UnityEngine.AI; // 내비메쉬 관련 코드

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
        Vector3 spawnPosition = GetRandomPointOnMacMesh(playerTransform.position, maxDistance); // 플레이어 근처에서 내비메시 위의 랜덤 위치 가져오기
        spawnPosition += Vector3.up * 0.5f; // 바닥에서 0.5만큼 위로 올리기

        // 아이템 중 하나를 무작위로 골라 위치에 생성
        GameObject selectedItem = items[Random.Range(0, items.Length)];
        GameObject item = Instantiate(selectedItem, spawnPosition, Quaternion.identity);

        Destroy(item, 5f); // 생성된 아이템을 5초 뒤에 파괴
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