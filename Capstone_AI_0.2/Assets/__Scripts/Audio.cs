using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    public int audioType;

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
		audioSource = GetComponent<AudioSource>();
	}

	private void Update()
	{
		audioSource.volume = GameManager.Instance.GetVolume(audioType);
	}
}
