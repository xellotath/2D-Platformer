using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class GameUI : MonoBehaviour {
	[SerializeField] private Image heart1;
	[SerializeField] private Image heart2;
	[SerializeField] private Image heart3;

	[SerializeField] private TMP_Text timeText;
	[SerializeField] private TMP_Text gameEndText;
	[SerializeField] private GameObject gameOverScreen;
	[SerializeField] private GameObject gameEndScreen;
	[SerializeField] private GameObject pausedScreen;

	private GameManager gameManager;

	// Start is called before the first frame update
	void Start() {
		gameEndScreen.SetActive(false);
		gameOverScreen.SetActive(false);
		gameManager = GameManager.Instance;
	}

	// Update is called once per frame
	void Update() {
		checkHealth();
		if (gameManager.gameOver) {
			gameOverScreen.SetActive(true);
		} else if (gameManager.gameState == GameManager.GameState.GAMEEND) {
			String endText = "Level Complete!!\n";
			endText += "Game Time: " + gameManager.levelTimeSpan(false) + "\n";
			endText += "Humans Killed: " + gameManager.enemiesKilled + "\n";
			endText += "No.of Jumps: " + gameManager.noJumps + "\n";
			endText += "No.of attacks: " + gameManager.noAttacks + "\n";
			gameEndText.text = endText;
			gameEndScreen.SetActive(true);
		}

		if (gameManager.gameState == GameManager.GameState.PAUSED) {
			pausedScreen.SetActive(true);
		} else {
			pausedScreen.SetActive(false);
		}
		timeText.text = gameManager.levelTimeSpan();
		if (gameManager.godMode) timeText.text += " GOD";
	}

	public void Resume() {
		gameManager.gameState = GameManager.GameState.GAMEPLAY;
	}

	public void Pause() {
		gameManager.gameState = GameManager.GameState.PAUSED;
	}

	void checkHealth() {
		switch (gameManager.playerHealth) {
			case 0:
				heart1.enabled = false;
				heart2.enabled = false;
				heart3.enabled = false;
				break;
			case 1:
				heart1.enabled = false;
				heart2.enabled = false;
				heart3.enabled = true;
				break;
			case 2:
				heart1.enabled = false;
				heart2.enabled = true;
				heart3.enabled = true;
				break;
			case 3:
				heart1.enabled = true;
				heart2.enabled = true;
				heart3.enabled = true;
				break;
		}
	}

	public void gameOverRetry() {
		gameManager.ResetLevel();
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void mainMenu() {
		gameManager.ResetLevel();
		SceneManager.LoadScene(0);
	}
}
