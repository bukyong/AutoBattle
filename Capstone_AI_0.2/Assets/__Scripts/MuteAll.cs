using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteAll : MonoBehaviour
{
	public Slider Total;
	public Slider Background;
	public Slider Effect;

	public void ChangeMute()
	{
		if(GameManager.Instance.Change_Mute())
		{
			Total.value = 0;
			Background.value = 0;
			Effect.value = 0;
		}
		else
		{
			Total.value = GameManager.Instance.Sound_MuteTotal;
			Background.value = GameManager.Instance.Sound_MuteBackground;
			Effect.value = GameManager.Instance.Sound_MuteEffect;
		}
	}
}
