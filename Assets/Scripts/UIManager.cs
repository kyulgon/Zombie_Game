using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리자 관련 코드
using UnityEngine.UI; // UI 관련 코드

// 필요한 UI에 즉시 접근하고 변경할 수 있도록 허용하는 UI 매니저
public class UIManager : MonoBehaviour
{
   public static UIManager instance // 싱글턴 접근용 프로퍼티
    {
        get
        {
            if(m_instance == null) // 
            {
                m_instance = FindObjectOfType<UIManager>();
            }

            return m_instance;
        }
    }

    private static UIManager m_instance; // 싱글턴이 할당될 변수

    public Text ammoText; // 탄알 표시용 텍스트
    public Text scoreText; // 점수 표시용 텍스트
    public Text waveText; // 적 웨이브 표시용 텍스트
    public GameObject gameoverUI; // 게임어보 시 활성화할 UI

    public void UpdateAmmoText(int magAmmo, int remainAmmo) // 탄알 텍스트 갱신
    {
        ammoText.text = magAmmo + "/" + remainAmmo;
    }

    public void UpdateScoreText(int newScore) // 점수 텍스트 갱신
    {
        scoreText.text = "Score : " + newScore;
    }

    public void UpdateWaveText(int waves, int count) // 적 웨이브 텍스트 갱신
    {
        waveText.text = "Wave " + waves + "\nEnemy Left :" + count;
    }

    public void SetActiveGameoverUI(bool active) // 게임오버 UI 활성화
    {
        gameoverUI.SetActive(active);
    }

    public void GameRestart() // 게임 재시작
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }




}