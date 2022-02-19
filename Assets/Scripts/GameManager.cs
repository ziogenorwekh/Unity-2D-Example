using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove playerMove;
    public GameObject[] Stages;

    // UI system
    public Image[] UIhealth;
    public Text UIPoint;
    public Text UIStage;
    public GameObject UIRestartBtn;

    void Update() {
        UIPoint.text = (totalPoint + stagePoint).ToString(); // 점수를 계속 업데이트
    }
    public void NextStage()
    {
        if(stageIndex< Stages.Length-1) {
        // 스테이지 변경
        // 다음 스테이지로 int값 하나 증가
        Stages[stageIndex].SetActive(false);
        stageIndex++;
        Stages[stageIndex].SetActive(true);

        UIStage.text = "STAGE " + (stageIndex+1);
        PlayerReposition();
        }
        else { // 게임 클리어
            // 완주하면 게임 시간 0
            Time.timeScale = 0;
            // 로그
            Debug.Log("게임 클리어!");
            // 재시작 UI
            Text btnText = UIRestartBtn.GetComponentInChildren<Text>(); // 버튼 텍스트는 자손이므로 InChildren
            btnText.text = "Game Clear!";
            UIRestartBtn.SetActive(true);

        }
        // total 포인트에 stagePoint를 주고 초기화
        totalPoint += stagePoint;
        stagePoint = 0;
    }
    public void HealthDown()
    {
        if (health > 1)
        {
            health--;
            UIhealth[health].color = new Color(1,0,0,0.4f); // 체력이 하나 까이고 빨간색 옅게
        }
        else
        {
            // 모든 체력 바 UI 끈다.
            UIhealth[0].color = new Color(1,0,0,0.4f);
            // 플레이어 사망 효과
            playerMove.OnDie();
            // 결과창 UI
            Debug.Log("죽었다");
            // 재시작 버튼 UI
            UIRestartBtn.SetActive(true);
        }
    }
    void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.gameObject.tag == "Player")
        {

            if (health > 1)
            {
                // 플레이어 원위치, 마지막 체력에서 낭떨어지로 갈 경우, 원 위치 안한다.
                PlayerReposition();
            }
            // 체력 하나 깎는다.
            HealthDown();
        }
    }
    void PlayerReposition()
    {
        playerMove.transform.position = new Vector3(-10.5f, 0.5f, 0);
        playerMove.VelocityZero();
    }

    public void Restart() {
        Time.timeScale = 1; // 재시작하게 되면 timeScale = 1로 시간으로 복구
        SceneManager.LoadScene(0);
    }
}
