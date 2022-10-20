using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
	public GameObject P_Option;
	public GameObject P_Menu;

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

	public void CS_Title()
	{
		SceneManager.LoadScene("Title");
	}

	public void CS_Stage()
	{
		SceneManager.LoadScene("Stage");
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
