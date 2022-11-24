using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MagicianAI : LivingEntity
{
    public LayerMask whatIsTarget; // 추적 대상의 레이어

    private LivingEntity targetEntity; // 추적 대상
    private NavMeshAgent pathFinder; // 경로 계산 AI 에이전트
    private Animator playerAnimator; // 플레이어 애니메이션

    public float damage = 40f; // 공격력
    public float defense = 0f; // 방어력
    public float attackDelay = 3f; // 공격 딜레이
    public int attackStack = 0; // 공격 스택 (임시, 마나로 대체할 수도 있음)

    private float attackRange = 10f; // 공격 사거리
    private float lastAttackTime; // 마지막 공격 시점
    private float dist; // 추적대상과의 거리

    public Transform tr;

    // 원거리 공격
    public GameObject firePoint; // 매직미사일이 발사될 위치
    public GameObject magicMissilePrefab; // 사용할 매직미사일 할당
    public GameObject magicMissile; // Instantiate() 메서드로 생성하는 매직미사일을 담는 게임오브젝트

    public GameObject pgoHpBar; // 체력 바
    public GameObject hpBarPrefab; // 체력 바 프리팹 할당

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
    }

    // AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(float newHealth, float newDamage, float newDefense)
    {
        // 체력 설정
        startingHealth = newHealth;
        Health = newHealth;
        // 공격력 설정
        damage = newDamage;
        // 방어력 설정
        defense = newDefense;
        // 네비메쉬 에이전트의 이동 속도 설정
        //pathFinder.speed = newSpeed;
    }

    void Start()
    {
        // 게임 오브젝트 활성화와 동시에 AI의 탐지 루틴 시작
        //Setup(100f, 40f, 0f);
        StartCoroutine(UpdatePath());
        tr = GetComponent<Transform>();
        pgoHpBar = Instantiate(hpBarPrefab);
        pgoHpBar.transform.SetParent(GameObject.Find("Canvas").transform);
        pgoHpBar.GetComponentInChildren<HpBar>().MaxHP = base.Health;
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
        pgoHpBar.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0f, 1.0f, 0.3f));
        pgoHpBar.GetComponentInChildren<HpBar>().HP = base.Health;
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

                    // 지정된 반지름 크기의 콜라이더로 whatIsTarget 레이어를 가진 콜라이더 검출하기
                    Collider[] colliders = Physics.OverlapSphere(transform.position, 30f, whatIsTarget);

                    // 콜라이더가 검출이 되면 거리 비교를 통해 가장 가까운 적을 타겟으로 변경
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
            }
			// 0.25초 주기로 처리 반복
			yield return new WaitForSeconds(0.25f);
		}
    }

    // 적과 플레이어 사이의 거리 측정, 거리에 따라 공격 실행
    public virtual void Attack()
    {
        // 자신이 사망X, 최근 공격 시점에서 공격 딜레이 이상 시간이 지났고,
        // 플레이어와의 거리가 공격 사거리안에 있다면 공격 가능
        if (!Dead && dist <= attackRange)
        {
            // 공격 반경 안에 있으면 움직임을 멈춤
            isMove = false;
            pathFinder.isStopped = true;

            // 최근 공격 시점에서 공격 딜레이 이상 시간이 지나면 공격 가능
            if (lastAttackTime + attackDelay <= Time.time)
            {
                isAttack = true;
                Debug.Log("마법사 공격 실행");
                lastAttackTime = Time.time;  // 최근 공격시간 갱신
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
            // 추적 대상이 존재하고 추적 대상이 공격 반경 밖에 있을 경우,
            // 경로를 갱신하고 AI 이동을 계속 진행
            isMove = true;
            isAttack = false;
            pathFinder.isStopped = false;
            pathFinder.SetDestination(targetEntity.transform.position);
        }
    }

    // 매직 미사일 생성
    public void Fire()
    {
        // Instatiate()로 매직 미사일 프리팹을 복제 생성
        magicMissile = Instantiate(magicMissilePrefab, firePoint.transform.position, firePoint.transform.rotation);

        attackStack += 2;
        playerAnimator.SetInteger("Skill", attackStack);

        // 공격이 되는지 확인하기 위한 디버그 출력
        Debug.Log("매직미사일 생성!");
    }

    public void MagicianSkillAOE()
    {
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        Debug.Log("마법사 광역기 스킬 사용!");

        attackStack = 0;
        playerAnimator.SetInteger("Skill", attackStack);
    }

    public void MagicianSkillHeal()
    {
        Debug.Log("마법사 회복 스킬 사용!");
        
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, LayerMask.GetMask("Player"));

        foreach (Collider heal in colliders)
        {
            LivingEntity healTarget = heal.gameObject.GetComponent<LivingEntity>();
            healTarget.Heal(10);
        }
        
        attackStack = 0;
        playerAnimator.SetInteger("Skill", attackStack);
    }

    // 데미지를 입었을 때 실행할 처리
    public override void OnDamage(float damage)
    {
        // LivingEntity의 OnDamage()를 실행하여 데미지 적용
        if (damage - defense <= 0)
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
        Debug.Log("마법사 사망...");

        // 게임오브젝트 비활성화
        gameObject.SetActive(false);
        pgoHpBar.SetActive(false);
        //Destroy(gameObject);
    }
}