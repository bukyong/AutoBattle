using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sound_Slider : MonoBehaviour
{
    Slider slider;
    
    public int sliderType;

    // Start is called before the first frame update
    void Awake()
    {
        slider= GetComponent<Slider>();

		slider.value = GameManager.Instance.Synchronization_Sound(sliderType);
	}
}
