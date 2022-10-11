using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAIRange : LivingEntity
{
    public LayerMask whatIsTarget; // 추적대상 레이어

    private LivingEntity targetEntity; // 추적대상
    private NavMeshAgent pathFinder; // 경로 계산 AI 에이전트

    public float damage = 20f; // 공격력
    public float attackDelay = 2.5f; // 공격 딜레이
    private float lastAttackTime; // 마지막 공격 시점
    private float dist; // 추적대상과의 거리

    public Transform tr;

    // 원거리 공격
    public GameObject firePoint; // 매직미사일이 발사될 위치
    public GameObject magicMissilePrefab; // 사용할 매직미사일 할당
    public GameObject magicMissile; // Instantiate() 메서드로 생성하는 매직미사일을 담는 게임오브젝트

    private float attackRange = 10f; // 공격 사거리

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

    public bool canMove;
    public bool canAttack;

    private void Awake()
    {
        // 게임 오브젝트에서 사용할 컴포넌트 가져오기
        pathFinder = GetComponent<NavMeshAgent>();
    }

    // AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(float newHealth, float newDamage, float newSpeed)
    {
        // 체력 설정
        startingHealth = newHealth;
        Health = newHealth;
        // 공격력 설정
        damage = newDamage;
        // 네비메쉬 에이전트의 이동 속도 설정
        pathFinder.speed = newSpeed;
    }

    void Start()
    {
        // 게임 오브젝트 활성화와 동시에 AI의 탐지 루틴 시작
        StartCoroutine(UpdatePath());
        tr = GetComponent<Transform>();
    }

    void Update()
    {
        if (hasTarget)
        {
            // 추적 대상이 존재할 경우 거리 계산은 실시간으로 해야하니 Update()
            dist = Vector3.Distance(tr.position, targetEntity.transform.position);
        }
    }

    // 추적할 대상의 위치를 주기적으로 찾아 경로 갱신
    IEnumerator UpdatePath()
    {
        // 살아 있는 동안 무한 루프
        while (!Dead)
        {
            if (hasTarget)
            {
                Attack();
            }
            else
            {
                // 추적 대상이 없을 경우, AI 이동 정지
                pathFinder.isStopped = true;
                canAttack = false;
                canMove = false;

                // 반지름 20f의 콜라이더로 whatIsTarget 레이어를 가진 콜라이더 검출하기
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);

                // 모든 콜라이더를 순회하면서 살아 있는 LivingEntity 찾기
                for (int i = 0; i < colliders.Length; i++)
                {
                    // 콜라이더로부터 LivingEntity 컴포넌트 가져오기
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    // LivingEntity 컴포넌트가 존재하며, 해당 LivingEntity가 살아 있다면
                    if (livingEntity != null && !livingEntity.Dead)
                    {
                        // 추적 대상을 해당 LivingEntity로 설정
                        targetEntity = livingEntity;

                        // for문 루프 즉시 정지
                        break;
                    }
                }
            }

            // 0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    // 적과 플레이어 사이의 거리 측정, 거리에 따라 공격 메서드 실행
    public virtual void Attack()
    {
        // 자신이 사망X, 최근 공격 시점에서 attackDelay 이상 시간이 지났고, 플레이어와의 거리가 공격 사거리안에 있다면 공격 가능
        if (!Dead && dist <= attackRange)
        {
            // 공격 반경 안에 있으면 움직임을 멈춘다.
            canMove = false;

            this.transform.LookAt(targetEntity.transform);

            // 공격 딜레이가 지났다면 공격 애니 실행
            if (lastAttackTime + attackDelay <= Time.time)
            {
                canAttack = true;
                lastAttackTime = Time.time;  // 최근 공격시간 초기화
            }

            // 공격 반경 안에 있지만, 딜레이가 남아있을 경우
            else
            {
                canAttack = false;
            }
        }

        // 공격 반경 밖에 있을 경우 추적하기
        else
        {
            // 추적 대상이 존재 && 추적 대상이 공격 반경 밖에 있을 경우, 경로를 갱신하고 AI 이동을 계속 진행
            canMove = true;
            canAttack = false;
            pathFinder.isStopped = false;
            // 계속 이동
            pathFinder.SetDestination(targetEntity.transform.position);
        }
    }

    //유니티 애니메이션 이벤트로 지팡이를 앞으로 휘두를 떄 메서드 실행
    public void Fire()
    {
        magicMissile = Instantiate(magicMissilePrefab, firePoint.transform.position, firePoint.transform.rotation); //Instatiate()로 매직 미사일 프리팹을 복제 생성한다.
    }

    /*미사일에서 데미지 처리하기
    
    //유니티 애니메이션 이벤트로 휘두를 때 데미지 적용시키기
    public void OnDamageEvent()
    {
        //상대방의 LivingEntity 타입 가져오기
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        //공격 처리
        attackTarget.OnDamage(damage);
    }
    */
    // 유니티 애니메이션 이벤트로 휘두를 때 데미지 적용시키기
    public void OnDamageEvent()
    {
        // 공격 대상을 지정할 추적 대상의 LivingEntity 컴포넌트 가져오기
        LivingEntity attackTarget = targetEntity.GetComponent<LivingEntity>();

        // 공격 처리
        attackTarget.OnDamage(damage);

        // 공격이 되는지 확인하기 위한 디버그 출력
        Debug.Log("공격!");

        // 최근 공격 시간 갱신
        lastAttackTime = Time.time;
    }

    // 데미지를 입었을 때 실행할 처리
    public override void OnDamage(float damage)
    {
        // LivingEntity의 OnDamage()를 실행하여 데미지 적용
        base.OnDamage(damage);
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
    }
}