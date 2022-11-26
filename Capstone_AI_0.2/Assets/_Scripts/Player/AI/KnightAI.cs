using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KnightAI : LivingEntity
{
    public LayerMask whatIsTarget; // 추적 대상의 레이어

    private LivingEntity targetEntity; // 추적 대상
    private NavMeshAgent pathFinder; // 경로 계산 AI 에이전트
    private Animator playerAnimator; // 플레이어 애니메이션

    public Transform tr;

    public float damage; // 공격력
    public float defense; // 방어력
    public float attackDelay = 1f; // 공격 딜레이

    private float attackRange = 2f; // 공격 사거리
    private float lastAttackTime; // 마지막 공격 시점
    private float dist; // 추적대상과의 거리

    // 게이지
    public GameObject pgoGauge; // 유닛의 체력,마나 바
    public GameObject gaugePrefab; // 체력,마나 바 프리팹 할당

    // 추적 대상이 존재하는지 알려주는 프로퍼티
    private bool hasTarget
    {
        get
        {
            // 추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
            if (targetEntity != null && !targetEntity.Dead)
            {
                return true;
            }

            // 그렇지 않다면 false
            return false;
        }
    }

    // 애니메이션 실행 조건을 위한 변수
    public bool isMove;
    public bool isAttack;

    private void Awake()
    {
        // 게임 오브젝트에서 사용할 컴포넌트 가져오기
        pathFinder = GetComponent<NavMeshAgent>();
        playerAnimator = GetComponent<Animator>();
        Setup(200f, 10f, 10f, 5f);
        SetGauge();
    }

    // AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(float newHealth, float newMana, float newDamage, float newDefense)
    {
        // 체력 설정
        MaxHealth = newHealth;
        startingHealth = newHealth;
        // 마나 설정
        MaxMana = newMana;
        Mana = 0f;
        // 공격력 설정
        damage = newDamage;
        // 방어력 설정
        defense = newDefense;
        // 네비메쉬 에이전트의 이동 속도 설정
        //pathFinder.speed = newSpeed;
    }

    public void SetGauge()
    {
        pgoGauge = Instantiate(gaugePrefab);
        pgoGauge.name = gameObject.name + "_Gauge";
        pgoGauge.transform.SetParent(GameObject.Find("Canvas").transform);
        pgoGauge.GetComponentInChildren<HpBar>().MaxHP = base.MaxHealth;
        pgoGauge.GetComponentInChildren<MpBar>().MaxMP = base.MaxMana;
    }

    void Start()
    {
        // 게임 오브젝트 활성화와 동시에 AI의 탐지 루틴 시작
        StartCoroutine(UpdatePath());
        tr = GetComponent<Transform>();
    }

    void Update()
    {
        playerAnimator.SetBool("isMove", isMove);
        playerAnimator.SetBool("isAttack", isAttack);

        if (GameManager.Instance.isBattle)
        {
            if (hasTarget)
            {
                // 추적 대상이 존재할 경우 거리 계산은 실시간으로 해야하니 Update()에 작성
                dist = Vector3.Distance(tr.position, targetEntity.transform.position);

                // 추적 대상을 바라볼 때 기울어짐을 방지하기 위해 Y축을 고정시킴
                Vector3 targetPosition = new Vector3(targetEntity.transform.position.x, this.transform.position.y, targetEntity.transform.position.z);
                this.transform.LookAt(targetPosition);
            }
        }

        // 오브젝트위에 체력 바가 따라다님
        pgoGauge.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 0.5f, 0.5f));
        pgoGauge.GetComponentInChildren<HpBar>().HP = base.Health;
        pgoGauge.GetComponentInChildren<MpBar>().MP = base.Mana;
    }

    // 추적할 대상의 위치를 주기적으로 찾아 경로 갱신
    IEnumerator UpdatePath()
    {
        // 살아 있는 동안 무한 루프
        while (!Dead)
        {
            if (GameManager.Instance.isBattle)
            {
                if (hasTarget)
                {
                    Attack();
                }
                else
                {
                    // 추적 대상이 없을 경우, AI 이동 정지
                    pathFinder.isStopped = true;
                    isAttack = false;
                    isMove = false;

                    TargetSearch();
                }
            }

            // 0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void TargetSearch()
    {
        // 지정된 반지름 크기의 콜라이더로 whatIsTarget 레이어를 가진 콜라이더 검출하기
        Collider[] colliders = Physics.OverlapSphere(transform.position, 30f, whatIsTarget);

        // 만약 콜라이더가 검출이 되면 거리 비교를 통해 가장 가까운 적을 타겟으로 변경
        if (colliders.Length > 0)
        {
            GameObject target;
            target = colliders[0].gameObject;

            for (int i = 0; i < colliders.Length; i++)
            {
                if (Vector3.Distance(target.transform.position, this.transform.position) > Vector3.Distance(this.transform.position, colliders[i].transform.position))
                {
                    target = colliders[i].gameObject;
                    //break;
                }
            }

            targetEntity = target.GetComponent<LivingEntity>();
        }
    }

    // 적과 플레이어 사이의 거리 측정, 거리에 따라 공격 실행
    public virtual void Attack()
    {
        // 자신이 사망X, 최근 공격 시점에서 공격 딜레이 이상 시간이 지났고,
        // 플레이어와의 거리가 공격 사거리안에 있다면 공격 가능
        if (!Dead && dist < attackRange)
        {
            // 공격 반경 안에 있으면 움직임을 멈춤
            isMove = false;
            pathFinder.isStopped = true;

            // 최근 공격 시점에서 공격 딜레이 이상 시간이 지나면 공격 가능
            if (lastAttackTime + attackDelay <= Time.time)
            {
                isAttack = true;
            }
            // 공격 사거리 안에 있지만, 공격 딜레이가 남아있을 경우
            else
            {
                isAttack = false;
            }
        }
        // 공격 사거리 밖에 있을 경우 추적하기
        else
        {
            // 추적 대상이 존재하고 추적 대상이 공격 사거리 밖에 있을 경우,
            // 경로를 갱신하고 AI 이동을 계속 진행
            isMove = true;
            isAttack = false;
            pathFinder.isStopped = false;
            pathFinder.SetDestination(targetEntity.transform.position);
        }
    }

    // 기사 버프 스킬 메소드 (방어력 증가)
    public void KnightSkillBuff()
    {
        Debug.Log("기사 버프 스킬 사용!");

        StartCoroutine(OnBuffCoroutine(5, 3f));

        Mana = 0;
        playerAnimator.SetInteger("Mana", (int)Mana);
    }

    // 버프를 위한 코루틴 (버프 시간, 버프 증가량)
    IEnumerator OnBuffCoroutine(int time, float value)
    {
        // 방어력 증가를 한 번만 하고 설정된 타이머가 다 되면 방어력 감소
        defense += value;
        Debug.Log("기사 방어력 증가!");

        while (time > 0)
        {
            time--;
            //Debug.Log(time);
            yield return new WaitForSeconds(1f);
        }

        defense -= value;
        Debug.Log("기사 방어력 감소!");
    }

    // 데미지 처리하기
    // (유니티 애니메이션 이벤트로 휘두를 때 데미지 적용)
    public void OnDamageEvent()
    {
        // 상대방의 LivingEntity 타입 가져오기
        // (공격 대상을 지정할 추적 대상의 LivingEntity 컴포넌트 가져오기)
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        // 공격이 되는지 확인하기 위한 디버그 출력
        Debug.Log("기사 공격 실행");

        Mana += 1;
        playerAnimator.SetInteger("Mana", (int)Mana);
        attackTarget.OnDamage(damage);

        // 최근 공격 시간 갱신
        lastAttackTime = Time.time;
    }

    // 데미지를 입었을 때 실행할 처리
    public override void OnDamage(float damage)
    {
        // LivingEntity의 OnDamage()를 실행하여 데미지 적용
        if (damage - defense <= 0 )
        {
            base.OnDamage(0);
        }
        else
        {
            base.OnDamage(damage - defense);
        }

        // 피격 애니메이션 재생
        // playerAnimator.SetTrigger("Hit");
    }

    // 사망 처리
    public override void Die()
    {
        // LivingEntity의 DIe()를 실행하여 기본 사망 처리 실행
        base.Die();

        // 다른 AI를 방해하지 않도록 자신의 모든 콜라이더를 비활성화
        Collider[] enemyColliders = GetComponents<Collider>();
        for (int i = 0; i < enemyColliders.Length; i++)
        {
            enemyColliders[i].enabled = false;
        }

        // AI추적을 중지하고 네비메쉬 컴포넌트를 비활성화
        pathFinder.isStopped = true;
        pathFinder.enabled = false;

        // 사망 애니메이션 재생
        playerAnimator.SetTrigger("Die");
    }

    public void OnDie()
    {
        Debug.Log("기사 사망...");

        // 게임오브젝트 비활성화
        gameObject.SetActive(false);
        pgoGauge.SetActive(false);
        //Destroy(gameObject);
    }
}