using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class touch : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject Setunit;
  //  public Canvas m_canvas;
  //  public GraphicRaycaster m_gr;
 //   public PointerEventData m_ped;

     void Start()
    {
     //   m_gr = m_canvas.GetComponent<GraphicRaycaster>();
     //   m_ped = new PointerEventData(null);
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;


        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(ray, out hit))
            {

                if (hit.transform.gameObject.tag == "Player" && Setunit == null)
                {
                    print(hit.transform.gameObject.tag);
                    Setunit = hit.transform.gameObject;
                    Setunit.GetComponent<unitcheck>().clickunit();
                }
                if (hit.transform.gameObject.tag == "ui" )
                {
                    print(hit.transform.gameObject.tag);
                }
                    Setunit.transform.position = ray.GetPoint(hit.distance);
                Setunit.transform.position = new Vector3(Setunit.transform.position.x, 0.7f, Setunit.transform.position.z);
            }
             
          
        }
        else
        {
            if (Setunit != null)
            {
                if( Setunit.GetComponent<unitcheck>().matterunit!=null)
                {
                   
                if(Setunit.GetComponent<unitcheck>().rank== Setunit.GetComponent<unitcheck>().matterunit.GetComponent<unitcheck>().rank)
                    {
                        Destroy(Setunit.GetComponent<unitcheck>().matterunit);
                        Setunit.GetComponent<unitcheck>().matterunit = null;
                        Setunit.GetComponent<unitcheck>().rank+=1;
                        print(Setunit.GetComponent<unitcheck>().rank);
                    }
                    if (Setunit.GetComponent<unitcheck>().matterunit != null)
                        Setunit.transform.position += new Vector3(0, 0, 2);
                }

               Setunit.GetComponent<unitcheck>().unclickunit();
                Setunit = null;
            }
        }
    }
    

}
