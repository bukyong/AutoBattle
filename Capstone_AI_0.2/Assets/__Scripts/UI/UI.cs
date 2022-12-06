using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
	public GameObject P_Option;
	public GameObject P_Menu;
	public GameObject P_GameOver;
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

	public void CS_Title()
	{
		SceneManager.LoadScene("Title");
	}

	public void CS_Stage()
	{
		SceneManager.LoadScene("GamePlay");
	}

	public void BattleStart()
	{
		if(GameManager.Instance.GetGameStage() == GameState.BeforeBattle)
		{
			GameManager.Instance.ChangeGameState();
		}
		
		Debug.Log("��������");
	}

	public void PauseGame()
	{
		Time.timeScale = 0.0f;
	}

	public void ResumGame()
	{
		Time.timeScale = GameManager.Instance.GameSpeed;
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
