using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;

// 적 게임 오브젝트를 주기적으로 생성
public class EnemySpawner : MonoBehaviourPun, IPunObservable
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

    private int enemyCount = 0; // 현재 남은 적 수
    private int wave; // 현재 웨이브

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) // 주기적으로 자동 실행되는 동기화 메서드
    {
        if(stream.IsWriting) // 로컬 오브젝트라면 쓰기 부분이 실행됨
        {
            stream.SendNext(enemies.Count); // 남은 적 수를 네트워크를 통해 보내기
            stream.SendNext(wave); // 현재 웨이브를 네트워크를 통해 보내기
        }
        else // 리모트 오브젝트라면 읽기 부분이 실행됨
        {
            enemyCount = (int)stream.ReceiveNext(); // 남은 적 수를 네트워크를 통해 받기
            wave = (int)stream.ReceiveNext(); // 현재 웨이브를 네트워크를 통해 받기
        }
    }

    private void Awake()
    {
        PhotonPeer.RegisterType(typeof(Color), 128, ColorSerialization.SerializeColor, ColorSerialization.DeserializeColor);
    }


    private void Update()
    {
        // 호스트만 적을 직접 생성할 수 있음
        // 다른 클라이언트는 호스트가 생성한 적을 동기화를 통해 받아옴
        if(PhotonNetwork.IsMasterClient)
        {
            if (GameManager.instance != null && GameManager.instance.isGameover) //  게임오버 상태일 때는 생성하지 않음
            {
                return;
            }

            if (enemies.Count <= 0)
            {
                SpawnWave();
            }
        }           

        UpdateUI(); // UI 갱신
    }

    private void UpdateUI()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            UIManager.instance.UpdateWaveText(wave, enemies.Count); // 현재 웨이브와 남은 적 수 표시
        }
        else
        {
            // 클라이언트는 적 리스트를 갱신할 수 없으므로
            // 호스트가 보내준 enemyCount를 이용하여 적 수 표시
            UIManager.instance.UpdateWaveText(wave, enemyCount);
        }

        
    }

    private void SpawnWave() // 현재 웨이브에 맞춰 적 생성
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

        // 적 프리팹으로부터 적 생성. 네트워크상의 모든 클라이언트에 생성됨
        GameObject createEnemy = PhotonNetwork.Instantiate(enemyPrefab.gameObject.name, spawnPoint.position, spawnPoint.rotation);

        Enemy enemy = createEnemy.GetComponent<Enemy>(); // 생성한 적을 셋업하기 위해 Enemy 컴포넌트를 가져옴

        enemy.photonView.RPC("Setup", RpcTarget.All, health, damage, speed, skinColor); // 생성한 적의 능력치와 추적 대상 설정

        enemies.Add(enemy); // 생성된 적을 리스트에 추가

        // 적의 onDeath 이벤트에 익명 메서드 등록
        enemy.onDeath += () => enemies.Remove(enemy); // 사망한 적을 리스트에서 제거
        enemy.onDeath += () => StartCoroutine(DestroyAfter(enemy.gameObject, 10f)); // 사망한 적을 10초 뒤에 파괴
        enemy.onDeath += () => GameManager.instance.AddScore(100); // 적 사망 시 점수 상승


        IEnumerator DestroyAfter(GameObject target, float delay) // 포톤의 PhotonNetwork.Destry()는 지연 파괴를 지원하지 않으므로 지연 파괴를 직접 구현함
        {
            yield return new WaitForSeconds(delay); // delay만큼 쉬고

            if(target != null) // target이 아직 파괴되지 않았다면
            {
                PhotonNetwork.Destroy(target); // target을 모든 네트워크상에서 파괴
            }
        }

    }

    
}
   
