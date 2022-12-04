using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour
{
    private Rigidbody playerRigidbody;
    private Animator playerAnimator;

    public float speed = 5f;

    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            playerAnimator.SetBool("CanMove", true);
            transform.Translate(-speed * Time.deltaTime, 0, 0);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            playerAnimator.SetBool("CanMove", true);
            transform.Translate(speed * Time.deltaTime, 0, 0);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            playerAnimator.SetBool("CanMove", true);
            transform.Translate(0, 0, speed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            playerAnimator.SetBool("CanMove", true);
            transform.Translate(0, 0, -speed * Time.deltaTime);
        }
        else
        {
            playerAnimator.SetBool("CanMove", false);
        }
    }
}
