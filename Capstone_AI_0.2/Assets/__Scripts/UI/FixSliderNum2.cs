using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FixSliderNum2 : MonoBehaviour
{
	public Slider slider;
	public TextMeshProUGUI text;

	private void Update()
	{
		text.text = (slider.value).ToString("F1");
	}
}
