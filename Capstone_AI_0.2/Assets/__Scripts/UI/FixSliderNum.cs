using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FixSliderNum : MonoBehaviour
{
	public Slider slider;
	public TextMeshProUGUI text;

	private void Update()
	{
		text.text = ((int)(slider.value * 10)).ToString();
	}
}
