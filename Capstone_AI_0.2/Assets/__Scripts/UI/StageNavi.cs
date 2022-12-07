using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageNavi : MonoBehaviour
{
    Vector3 pos;


	// Start is called before the first frame update
	void Start()
    {
        pos = transform.position;
	}

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeStageNavi()
    {
        int stage = GameManager.Instance.Stage;

        if (stage == 1)
        {
			transform.DOLocalMoveX(83, 12f);
		}
        else if(stage == 2)
        {
			transform.DOLocalMoveX(148, 12f);
		}
		else if (stage == 3)
		{
			transform.DOLocalMoveX(208, 12f);
		}
        else if (stage == 4)
        {
            transform.position = pos;
        }
		if (stage == 5)
		{
			transform.DOLocalMoveX(83, 12f);
		}
		else if (stage == 6)
		{
			transform.DOLocalMoveX(148, 12f);
		}
		else if (stage == 7)
		{
			transform.DOLocalMoveX(208, 12f);
		}
	}
}
