using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Text_Chapter : MonoBehaviour
{
    TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

	private void Update()
	{
		int n = GameManager.Instance.Stage + 1;
		int i = 1;

		if (GameManager.Instance.Stage >= 4)
		{
			n = n - 4;
			i = 2;
		}

		text.text = "Chapter " + i.ToString() + " - " + n.ToString();
	}

}
