using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;   // ��ũ��Ʈ���� ������̼� �ý��� ����� ����Ϸ��� AI ���ӽ����̽��� using �����ؾ���

public class Moveable : MonoBehaviour
{
    // ���� ã�Ƽ� �̵��� ������Ʈ
    NavMeshAgent agent;

    public bool isCrash = false;

    // ������Ʈ�� ������
    [SerializeField]
    Transform target;

    private void Awake()
    {
        // ������ ���۵Ǹ� ���� ������Ʈ�� ������ NavMeshAgent ������Ʈ�� �����ͼ� ����
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!isCrash)
        {
            // ������Ʈ���� �������� �˷��ִ� �Լ�
            agent.SetDestination(target.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            isCrash = true;
        }

        Debug.Log("Target �浹!");
    }
}
