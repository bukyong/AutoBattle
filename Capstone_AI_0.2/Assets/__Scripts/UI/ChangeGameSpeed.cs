using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeGameSpeed : MonoBehaviour
{
	Slider slider;
	// Start is called before the first frame update
	void Start()
	{
		slider = GetComponent<Slider>();
	}

	private void Update()
	{
		GameManager.Instance.Change_GameSpeed(slider);
	}
}
