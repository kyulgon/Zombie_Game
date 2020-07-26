using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

// 점수와 게임 오버 여부를 관리하는 게임 매니저
public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    // 싱글턴 접근용 프로퍼티
    public static GameManager instance
    {
        get
        {
            if(m_instance == null) // 만약 싱글턴 변수에 아직 오브젝트가 할당되지 않았다면
            {
                m_instance = FindObjectOfType<GameManager>(); // 씬에서 GameManager 오브젝트를 찾아서 할당
            }

            return m_instance; // 싱글턴 오브젝트 반환
        }
    }

    private static GameManager m_instance; // 싱글턴이 할당될 static 변수

    public GameObject playerPrefab; // 생성할 프레이어 캐릭터 프리팹

    private int score = 0; // 현재 게임 점수
    public bool isGameover { get; private set; } // 게임오버 상태

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) // 주기적으로 자동 실행되는 동기화 메서드
    {
        if(stream.IsWriting)
        {
            stream.SendNext(score); // 네트워크를 통해 score 값 보내기
        }
        else // 리모트 옵즈젝트라면 읽기 부분이 실행됨
        {
            score = (int)stream.ReceiveNext(); // 네트워크를 통해 score 값 받기
            UIManager.instance.UpdateScoreText(score); // 동기화하여 받은 점수를 UI로 표시
        }
    }

    private void Awake()
    {
        if(instance != this) // 씬에 싱글턴 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        {
            Destroy(gameObject); // 자신을 파괴
        }
    }

    private void Start() // 게임 시작과 동시에 플레이어가 될 게임 오브젝트 생성
    {
        Vector3 randSpawnPos = Random.insideUnitSphere * 5f; // 생성할 랜덤 위치 지정
        randSpawnPos.y = 0f; // 위치 y값은 0으로 변경

        // 네트워크상의 모든 클라이언트에서 생성 실행
        // 해당 게임 오브젝트의 주도권은 생성 메서드를 직접 실행한 클라이언트에 있음
        PhotonNetwork.Instantiate(playerPrefab.name, randSpawnPos, Quaternion.identity);
    }

    public void AddScore(int newScore) // 점수를 추가하고 UI 갱신
    {
        if(!isGameover) // 게임오버가 아닌 상태에서만 점수 추가 기능
        {
            score += newScore; // 점수 추가
            UIManager.instance.UpdateScoreText(score); // 점수 UI 텍스트 갱신
        }
    }

    private void EndGame() // 게임오버 처리
    {
        isGameover = true; // 게임오버 상태를 참으로 변경
        UIManager.instance.SetActiveGameoverUI(true); // 게임오버 UI 활성화
    }

    private void Update() // 키보드 입력을 감지하고 룸을 나가게 함
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftLobby() // 룸을 나갈 때 자동 실행되는 메서드
    {
        SceneManager.LoadScene("Lobby"); // 룸을 나가면 로비 씬으로 돌아감
    }




}