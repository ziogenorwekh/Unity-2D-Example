using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    // Start is called before the first frame update
    public float maxSpeed;
    public float jumpPower;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    CapsuleCollider2D capsuleCollider2D;

    // Audios
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;
    AudioSource audioSource;
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>(); // 오디오 소스 컴포넌트
    }

    void PlaySound(string action) {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                audioSource.Play(); 
                break;
            case "ATTACK":
                audioSource.clip =  audioAttack;
                audioSource.Play(); 
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                audioSource.Play(); 
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                audioSource.Play(); 
                break;
            case "DIE":
                audioSource.clip = audioDie;
                audioSource.Play(); 
                break;
            case "FINISH":
                audioSource.clip = audioFinish;
                audioSource.Play(); 
                break;
        }
    }
    void Update()
    {
        // 점프 
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            // 점프하는 오디오를 재생한다.
            // audioSource.clip = audioJump;
            // audioSource.Play();
            PlaySound("JUMP");
        }
        if (Input.GetButtonUp("Horizontal"))
        {
            // Stop Speed x좌표축은 단위 벡터
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.00001f, rigid.velocity.y);
        }

        // 방향 전환
        if (Input.GetButton("Horizontal"))
        {
            // Stop Speed x좌표축은 단위 벡터
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        }
        {
            if (Mathf.Abs(rigid.velocity.x) < 0.3f) // 좌 우 는 값이 음수 양수 이므로, 절대값을 씌운다.
            {
                anim.SetBool("isWalking", false);
            }
            else
            {
                anim.SetBool("isWalking", true);
            }
        }
    }
    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal"); // Move by Key Control
        rigid.AddForce(Vector2.right * h/*2를 넣은 이유는 경사를 넘기 위해서*/, ForceMode2D.Impulse);
        if (rigid.velocity.x > maxSpeed) // 우 속도 x 좌표
        {
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y); // y값은 변동 없음 0을 넣을 경우에 점프 모션을 추가하면 정지해버린다.
        }
        else if (rigid.velocity.x < maxSpeed * (-1)) // 좌 속도 x 좌표, 좌는 -값이므로, -1을 곱해서 양수로 만들어준다.
        {
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y); // y값은 변동 없음 0을 넣을 경우에 점프 모션을 추가하면 정지해버린다.
        }

        // Landing Platfrom , DrawRay() - 에디터 상에서만 Ray를 그려주는 함수
        if (rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0)); // 에디터에서 보면 초록색 빔이 아래를 향하고 있는 모습
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.5f)
                {
                    anim.SetBool("isJumping", false);
                }
                // Debug.Log(rayHit.collider.name);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            // Attack
            if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
            }
            else
            {
                OnDamaged(collision.transform.position);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.gameObject.tag == "Item")
        { // 아이템과 충돌하면
            // Point
            // Sound
            PlaySound("ITEM");
            bool isBronze = collider2D.gameObject.name.Contains("Bronze"); // Bronze면
            bool isSilver = collider2D.gameObject.name.Contains("Silver"); // Silver면
            bool isGold = collider2D.gameObject.name.Contains("Gold"); // 게임 오브젝트의 이름이 Gold면

            if (isBronze)
            {
            gameManager.stagePoint += 50;
            } else if (isSilver) {
                gameManager.stagePoint += 100;
            } else if (isGold) {
                gameManager.stagePoint += 300;
            }
            // Deactive Item
            collider2D.gameObject.SetActive(false); // 활성화 중단
        }
        else if (collider2D.gameObject.tag == "Finish")
        {
            // Sound
            PlaySound("FINISH");
            // 다음 스테이지
            gameManager.NextStage();
        }
    }
    void OnAttack(Transform enemy)
    {
        // Sound
        PlaySound("ATTACK");
        // Point
        gameManager.stagePoint += 100;

        // reAction 위로 up
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        // Enemy Die EnemyMove에서 호출
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }
    void OnDamaged(Vector2 targetPos)
    {
        // Sound
        PlaySound("DAMAGED");
        // 체력 감소
        gameManager.HealthDown();
        // 레이어 변경
        gameObject.layer = 11;
        // 충돌 후 색상 변경
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // 튕겨나가는 액션
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);
        // 애니메이션
        anim.SetTrigger("doDamaged");


        Invoke("OffDamaged", 2);
    }
    void OffDamaged()
    {
        // 레이어 변경
        gameObject.layer = 10;
        // 충돌 후 색상 변경
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void OnDie() {
        // Sound
        PlaySound("DIE");
        // Sprite Alpha
        spriteRenderer.color = new Color(1,1,1,0.4f);
        // Sprite Flip Y
        spriteRenderer.flipY = true;
        // Collider Disable
        capsuleCollider2D.enabled = false;
        // Die Effect Jump
        rigid.AddForce(Vector2.up*5,ForceMode2D.Impulse);
    }
    public void VelocityZero() {
        rigid.velocity = Vector2.zero;
    }
}
