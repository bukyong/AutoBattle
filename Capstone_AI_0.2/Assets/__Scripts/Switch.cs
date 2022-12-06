using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
	public GameObject On;
	public GameObject Off;

	bool isOn = false;

	public void ChangeSwitch()
	{
		if(isOn == true)
		{
			On.SetActive(false);
			Off.SetActive(true);

			isOn = false;
		}
		else if(isOn == false)
		{
			On.SetActive(true);
			Off.SetActive(false);

			isOn = true;
		}
	}
}
