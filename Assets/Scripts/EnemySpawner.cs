using System.Collections.Generic;
using UnityEngine;

// 적 게임 오브젝트를 주기적으로 생성
public class EnemySpawner : MonoBehaviour
{
    public Enemy enemyPrefab; // 생성할 적 AI

    public Transform[] spawnPoints; // 적 AI를 소환할 위치

    public float damageMax = 40f;
    public float damageMin = 20f;

    public float healthMax = 200f;
    public float healthMin = 100f;

    public float speedMax = 3f;
    public float speedMin = 1f;

    public Color strongEnemyColor = Color.red; // 강한 적 AI가 가지게 될 피부색

    private List<Enemy> enemies = new List<Enemy>(); // 생성된 적을 담는 리스트
    private int wave; // 현재 웨이브

    private void Update()
    {
        if( GameManager.instance != null && GameManager.instance.isGameover) //  게임오버 상태일 때는 생성하지 않음
        {
            return;
        }

        if(enemies.Count <= 0)
        {
            SpawnWave();
        }

        UpdateUI(); // UI 갱신
    }

    private void UpdateUI()
    {
        UIManager.instance.UpdateWaveText(wave, enemies.Count); // 현재 웨이브와 남은 적 수 표시
    }

    private void SpawnWave()
    {
        wave++; // 웨이브 1 증가

        int spawnCount = Mathf.RoundToInt(wave * 1.5f); // 현재 웨이브 * 1.5를 반올림한 수만큼 적 생성

        for (int i = 0; i < spawnCount; i++) // spawnCount만큼 적 생성
        {
            float enemyIntensity = Random.Range(0f, 1f); // 적의 세기를 0% ~100% 사이에서 랜덤 결정

            CreateEnemy(enemyIntensity); // 적 생성 처리 실행
        }
    }
    private void CreateEnemy(float intensity)
    {
        // intensity를 기반으로 적의 능력치 결정
        float health = Mathf.Lerp(healthMin, healthMax, intensity);
        float damage = Mathf.Lerp(damageMin, damageMax, intensity);
        float speed = Mathf.Lerp(speedMin, speedMax, intensity);

        // intensity를 기반으로 하얀색과 enemyStrength 사이에서 적의 피부색 결정
        Color skinColor = Color.Lerp(Color.white, strongEnemyColor, intensity);

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)]; // 생성할 위치를 랜덤으로 결정

        Enemy enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation); // 적 프리팹으로부터 적 생성

        enemy.Setup(health, damage, speed, skinColor); // 생성한 적의 능력치와 추적 대상 설정

        enemies.Add(enemy); // 생성된 적을 리스트에 추가

        // 적의 onDeath 이벤트에 익명 메서드 등록
        enemy.onDeath += () => enemies.Remove(enemy); // 사망한 적을 리스트에서 제거
        enemy.onDeath += () => Destroy(enemy.gameObject, 10f); // 사망한 적을 10초 뒤에 파괴
        enemy.onDeath += () => GameManager.instance.AddScore(100); // 적 사망 시 점수 상승


    }

    
}
   
