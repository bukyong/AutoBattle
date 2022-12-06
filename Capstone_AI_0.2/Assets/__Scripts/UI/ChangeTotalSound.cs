using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTotalSound : MonoBehaviour
{
	Slider slider;
	// Start is called before the first frame update
	void Awake()
    {
        slider= GetComponent<Slider>();
    }

	private void Update()
	{
		GameManager.Instance.Change_Soundtotal(slider);
	}
}
