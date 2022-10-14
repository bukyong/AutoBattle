using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unitcheck : MonoBehaviour
{
    public bool click;
    public int rank;
    public GameObject matterunit;

    void Start()
    {
        rank = 1;
        click = false;
    }

 
    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player" && click == true&& matterunit==null)
        {
            matterunit = other.transform.gameObject;
            print(other.transform.gameObject.name);
        }    
    }

    private void OnTriggerExit(Collider other)
    {
   
        matterunit = null;

    }



    public void clickunit()
    {
        click = true;
        this.gameObject.GetComponent<BoxCollider>().isTrigger = true;
    }
    public void unclickunit()
    {
        click = false;
        matterunit = null;
        this.gameObject.GetComponent<BoxCollider>().isTrigger = false;
    }
}
