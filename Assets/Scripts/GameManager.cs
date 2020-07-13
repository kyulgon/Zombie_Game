﻿using UnityEngine;

// 점수와 게임 오버 여부를 관리하는 게임 매니저
public class GameManager : MonoBehaviour
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

    private int score = 0; // 현재 게임 점수
    public bool isGameover { get; private set; } // 게임오버 상태

    private void Awake()
    {
        if(instance != this) // 씬에 싱글턴 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        {
            Destroy(gameObject); // 자신을 파괴
        }
    }

    private void Start()
    {
        FindObjectOfType<PlayerHealth>().onDeath += EndGame; // 플레이어 캐릭터의 사망 이벤트 발생 시 게임오버
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

    

}