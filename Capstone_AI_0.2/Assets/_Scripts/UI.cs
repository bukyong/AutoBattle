using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
	public GameObject P_Option;
	public GameObject P_Menu;

	public GameObject L_Graphics;
	public GameObject L_Sound;

	public GameObject B_Pause;

	public void Active_Option()
	{
		P_Option.SetActive(true);
	}

	public void Close_Option()
	{
		P_Option.SetActive(false);
	}

	public void Active_Menu()
	{
		P_Menu.SetActive(true);
	}

	public void Close_Menu()
	{
		P_Menu.SetActive(false);
	}

	public void Active_Pause()
	{
		B_Pause.SetActive(true);
	}

	public void Close_Pause()
	{
		B_Pause.SetActive(false);
	}


	public void Active_L_Graphics()
	{
		L_Graphics.SetActive(true);
	}

	public void Close_L_Graphics()
	{
		L_Graphics.SetActive(false);
	}

	public void Active_L_Sound()
	{
		L_Sound.SetActive(true);
	}

	public void Close_L_Sound()
	{
		L_Sound.SetActive(false);
	}

	public void CS_Title()
	{
		SceneManager.LoadScene("Title");
	}

	public void CS_Stage()
	{
		SceneManager.LoadScene("GamePlay");
	}

	public void PauseGame()
	{
		Time.timeScale = 0.0f;
	}

	public void ResumGame()
	{
		Time.timeScale = 1.0f;
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
