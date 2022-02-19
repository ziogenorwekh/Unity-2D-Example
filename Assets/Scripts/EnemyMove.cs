using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;

    CircleCollider2D circleCollider2D;
    public int nextMove;
    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider2D = GetComponent<CircleCollider2D>();
        Invoke("Think", 3);
    }

    // Update is called once per frame
    void FixedUpdate() // 물리 기반이기에 FixedUpdate 사용해야 한다.
    {
        // Move
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);
        // Platform Check
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.2f, rigid.position.y); // Raycast를 오브젝트보다 한 칸 앞서게 벡터 선언
        Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0)); // 에디터에서 보면 초록색 빔이 아래를 향하고 있는 모습
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));
        if (rayHit.collider == null)
        {
            nextMove *= -1;
            if (nextMove != 0)
            {
                spriteRenderer.flipX = nextMove == 1;
            }
            CancelInvoke(); // 시간을 세고 있는 Invoke함수를 멈춘다.
            Invoke("Think", 3);
            // Turn();
        }
    }
    void Think()
    {
        // 다음 활동
        nextMove = Random.Range(-1, 2);
        // 스프라이트 애니메이션
        anim.SetInteger("WalkSpeed", nextMove);

        // 방향
        if (nextMove != 0)
        {
            spriteRenderer.flipX = nextMove == 1;
        }

        // 다음 행동 설정 재귀
        float nextThinkTime = Random.Range(2f, 4f);
        Invoke("Think", nextThinkTime);
    }
    void Turn() {
        nextMove *= -1;
        spriteRenderer.flipX = nextMove == 1;
        CancelInvoke(); // 시간을 세고 있는 Invoke함수를 멈춘다.
        Invoke("Think", 5);
    }
    public void OnDamaged() {
        // Sprite Alpha 색상 변경
        spriteRenderer.color = new Color(1,1,1,0.4f);
        // Sprite Flip Y 뒤집고
        spriteRenderer.flipY = true;
        // Collider Disable 충돌 collider 삭제
        circleCollider2D.enabled = false;
        // Die Effect Jump 죽는 모션
        rigid.AddForce(Vector2.up*5,ForceMode2D.Impulse);
        // Destory 오브젝트 제거
        Invoke("DeActive",5);
    }
    void DeActive() {
        gameObject.SetActive(false);
    }
}
