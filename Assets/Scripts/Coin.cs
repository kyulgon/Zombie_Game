using UnityEngine;
using Photon.Pun;

// 게임 점수를 증가시키는 아이템
public class Coin : MonoBehaviourPun, IItem {
    public int score = 200; // 증가할 점수

    public void Use(GameObject target)
    {        
        GameManager.instance.AddScore(score); // 게임 매니저로 접근해 점수 추가

        PhotonNetwork.Destroy(gameObject); // 모든 클라이언트에서 자신 파괴

    }
}