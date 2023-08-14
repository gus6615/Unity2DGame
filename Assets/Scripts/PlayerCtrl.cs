using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum PlayerState
{
    Idle,
    Walk,
    Evasion,
    Attack,
    Dead
}

public enum PlayerAttack
{
    None,
    Ready,
    BasicAttack_1,
    BasicAttack_2,
    StrongAttack
}

public class PlayerCtrl : MonoBehaviour
{
    private const float MP_CHARGE_SPEED = 1f;
    private const float EVASION_COOLTIME = 0.5f;
    private const float EVASION_FORCE = 2f;
    private const float STRONG_ATTACK_TIME = 3f;

    private static PlayerCtrl Instance;
    public static PlayerCtrl instance
    {
        set 
        {
            if (Instance == null)
                Instance = value; 
        }
        get { return Instance; }
    }

    private NavMeshAgent agent;
    private Animator animator;

    [SerializeField]
    private PlayerState playerState;
    
    public PlayerState state
    {
        set 
        {
            playerState = value;
            SetAnimation();
        }
        get { return playerState; }
    }

    /// <summary> 플레이어가 해당 기능을 사용할 수 있는 상태인가? </summary>
    private bool isCanMove, isCanAttack, isCanEvasion;


    /// <summary> 현재 공격 차징 중인가? </summary>
    private bool isAttackCharge;


    /// <summary> 플레이어 MAX HP </summary>
    public float max_HP;


    /// <summary> 플레이어 MAX MP </summary>
    public float max_MP;


    /// <summary> 최근 플레이어의 방향 벡터 </summary>
    private Vector2 moveVec;


    /// <summary> 최근 플레이어의 도착 벡터 </summary>
    private Vector2 goalVec;


    /// <summary> 현재 공격 Count 상태 </summary>
    private int attack_count;

    /// <summary> 현재 공격 상태라면 어떤 공격을 수행 중인가? </summary>
    private int attack_type;


    /// <summary> 공격 준비 시간 </summary>
    private float attack_clickTime;


    /// <summary> 플레이어 현재 HP </summary>
    private float CUR_HP;
    public float cur_HP
    {
        set 
        { 
            CUR_HP = value;
            PlayerStateUI.instance.SetPlayerHP();
        }
        get { return CUR_HP; }
    }


    /// <summary> 플레이어 현재 MP </summary>
    private float CUR_MP;
    public float cur_MP
    {
        set
        {
            CUR_MP = value;
            PlayerStateUI.instance.SetPlayerMP();
        }
        get { return CUR_MP; }
    }

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        agent= GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.updateUpAxis = false;
        agent.updateRotation = false;
        state = PlayerState.Idle;

        isCanMove = isCanAttack = isCanEvasion = true;
        isAttackCharge = false;
        max_HP = cur_HP = 100f;
        max_MP = cur_MP = 10f;
        moveVec = Vector2.right;
        attack_count = 0;
        attack_type = -1;
        attack_clickTime = 0f;

        animator.SetInteger("AttackType", attack_type);
    }

    // Update is called once per frame
    void Update()
    {
        if (isCanMove && Input.GetMouseButtonDown(0)) 
        {
            // 이동
            Vector3 mouseVec = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseVec.Set(mouseVec.x, mouseVec.y, -5f);
            if (!Physics2D.Raycast(mouseVec, Vector3.forward, 10f, 64))
            {
                // 갈 수 있는 지역을 누른 경우
                goalVec = mouseVec;
                if (isAttackCharge || attack_type != -1)
                    SetMove(goalVec, 1.5f); // 차징 중이거나 공격 중일 경우 느린 이동
                else
                    SetMove(goalVec, 3f);
            }
        }

        if (isCanAttack)
        {
            // 공격
            if (Input.GetKey(KeyCode.Q))
            {
                // 공격 버튼 누름
                attack_clickTime += Time.deltaTime;
                if (!isAttackCharge)
                {
                    // 공격 차징 시작
                    SetMoveSpeed(1.5f);
                    isAttackCharge = true;
                    isCanEvasion = false;
                    animator.SetBool("isAttack", true);
                }
            }
            else if (Input.GetKeyUp(KeyCode.Q))
            {
                // 공격 버튼 땜
                StartAttack();
            }
        }

        if (isCanEvasion && Input.GetKeyDown(KeyCode.W))
        {
            // 회피
            if (cur_MP >= 1f)
            {
                // 회피 스테미나 감소
                cur_MP -= 1f;

                // 회피 쿨타임 및 기능 수행
                StartCoroutine("EvasionCoolTime");
                StartEvasion();
            }
        }

        moveVec = goalVec - (Vector2)transform.position;

        // State에 따른 행동 수행
        StateFunc();

        // 마나 회복
        cur_MP += Time.deltaTime * MP_CHARGE_SPEED;
        if (cur_MP > max_MP)
            cur_MP = max_MP;
    }

    private void SetMove(Vector2 _destination, float _moveSpeed)
    {
        state = PlayerState.Walk;
        agent.SetDestination(_destination);
        SetMoveSpeed(_moveSpeed);
    }

    private void SetMoveSpeed(float _moveSpeed)
    {
        agent.speed = _moveSpeed;
    }

    /// <summary>
    /// 플레이어의 상태에 따른 기본 행동 요령을 수행하는 함수이다.
    /// </summary>
    private void StateFunc()
    {
        switch(state)
        {
            case PlayerState.Idle:
                break;
            case PlayerState.Walk:
                // 도착까지 남은 거리가 작으면 Idle로 변경
                if (moveVec.magnitude < Time.deltaTime)
                    state = PlayerState.Idle;
                else
                {
                    animator.SetFloat("DirX", agent.velocity.normalized.x);
                    animator.SetFloat("DirY", agent.velocity.normalized.y);
                }
                break;
        }
    }

    /// <summary>
    /// 플레이어의 상태에 따른 애니메이션을 설정하는 함수이다.
    /// </summary>
    private void SetAnimation()
    {
        switch (state)
        {
            case PlayerState.Idle:
                animator.SetBool("isWalk", false);
                animator.SetBool("isEvasion", false);
                break;
            case PlayerState.Walk:
                animator.SetBool("isWalk", true);
                animator.SetBool("isEvasion", false);
                break;
            case PlayerState.Evasion:
                animator.SetBool("isWalk", false);
                animator.SetBool("isEvasion", true);
                break;
            case PlayerState.Attack:
                break;
        }
    }

    /// <summary>
    /// 회피 쿨타임 시작 함수
    /// </summary>
    IEnumerator EvasionCoolTime()
    {
        isCanMove = isCanAttack = isCanEvasion = false;
        yield return new WaitForSeconds(EVASION_COOLTIME);
        isCanMove = isCanAttack = isCanEvasion = true;
    }

    /// <summary>
    /// 회피 상태로 설정하는 함수
    /// </summary>
    private void StartEvasion()
    {
        if (playerState.Equals(PlayerState.Idle))
            goalVec = (Vector2)transform.position - moveVec.normalized * EVASION_FORCE;
        else
            goalVec = (Vector2)transform.position - (Vector2)agent.velocity.normalized * EVASION_FORCE;
        SetMove(goalVec, 6f);
        state = PlayerState.Evasion;
    }

    /// <summary>
    /// 회피 애니메이션이 끝났음을 알리는 애니메이션 이벤트 함수 (Animator Tab에서 사용)
    /// </summary>
    public void EndEvasion()
    {
        SetMove(transform.position, 3f);
        state = PlayerState.Idle;
    }

    /// <summary>
    /// 현재 공격 상태 변수에 따라 특정 공격을 수행하는 함수이다.
    /// </summary>
    private void StartAttack()
    {
        if (attack_clickTime >= STRONG_ATTACK_TIME)
        {
            if (attack_count > 0)
            {
                // 콤보 공격
                attack_type = 2;
            }
            else
            {
                // 강한 공격
                attack_type = 1;
                cur_MP -= 3;
            }
            attack_count = 0;
        }
        else
        {
            // 일반 공격
            attack_type = 0;
            cur_MP -= 2;
            if (++attack_count > 2)
                attack_count = 0;
        }

        isCanAttack = false;
        isAttackCharge = false;
        attack_clickTime = 0f;
        playerState = PlayerState.Attack;
        animator.SetInteger("AttackType", attack_type);
    }

    public void EndAttack()
    {
        playerState = PlayerState.Idle;
        isCanAttack = true;
        isCanEvasion = true;
        attack_type = -1;
        animator.SetBool("isAttack", false);
        animator.SetInteger("AttackType", attack_type);
    }
}
