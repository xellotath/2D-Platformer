using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour {

	public static GameManager Instance { get; private set; }
	public int playerHealth { get; private set; }
	public int enemiesKilled = 0;
	public int noJumps = 0;
	public int noAttacks = 0;
	public bool gameOver { get; private set; }
	public bool godMode = false;
	DateTime levelStartTime;
	DateTime levelEndTime;
	public PlayerControls controls { get; private set; }

	public GameObject levelEndWayPoint = null;
	public GameState gameState = GameState.MAINMENU;

	private float godModePressCounter = 0.0f;
	public enum GameState {
		MAINMENU,
		GAMEPLAY,
		PAUSED,
		GAMEEND
	}

	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(this.gameObject);
		} else {
			Instance = this;
		}
		this.controls = new PlayerControls();
		controls.Enable();
		DontDestroyOnLoad(this);
		startLevel(levelEndWayPoint);
	}
	public void startLevel(GameObject levelEndWayPoint) {
		playerHealth = 3;
		enemiesKilled = 0;
		noJumps = 0;
		noAttacks = 0;
		gameOver = false;
		gameState = GameState.GAMEPLAY;
		this.levelEndWayPoint = levelEndWayPoint;
		levelStartTime = DateTime.Now;
	}


	// Start is called before the first frame update
	void Start() {
	}

	// Update is called once per frame
	void Update() {
		// pause
		bool pausedPressed = controls.UI.Pause.ReadValue<float>() > 0.0f;
		bool godModePressed = controls.Dev.God.ReadValue<float>() > 0.0f;
		if (pausedPressed)
			gameState = (gameState == GameState.GAMEPLAY) ? GameState.PAUSED : (gameState == GameState.PAUSED) ? GameState.GAMEPLAY : gameState;
		Time.timeScale = (gameState == GameState.PAUSED) ? 0 : 1;
		if (godModePressed && godModePressCounter <= 0.0f) {
			godModePressCounter = 0.3f;
			godMode = !godMode;
		} else
			godModePressCounter -= Time.deltaTime;

		// check for level complition
		if (levelEndWayPoint != null) {
			Collider2D[] colliders = Physics2D.OverlapCircleAll(levelEndWayPoint.transform.position, 1.0f);
			foreach (var collider in colliders) {
				if (collider.gameObject.tag == "Player") {
					SoundManager.Instance.playSound(SoundManager.SoundType.GameWin);
					levelEndTime = DateTime.Now;
					Destroy(collider.gameObject);
					gameState = GameState.GAMEEND;
				}
			}
		}
	}

	public int playerDamaged() {
		playerHealth--;
		if (playerHealth == 0) {
			gameOver = true;
			levelEndTime = DateTime.Now;
		}
		return playerHealth;
	}

	public String levelTimeSpan(bool currentTime = true) {
		DateTime endTime = (currentTime) ? DateTime.Now : levelEndTime;
		TimeSpan timeSpan = endTime - levelStartTime;
		return timeSpan.Minutes.ToString() + ":" + timeSpan.Seconds.ToString();
	}

	public void ResetLevel() {
		playerHealth = 3;
		enemiesKilled = 0;
		noJumps = 0;
		noAttacks = 0;
		gameOver = false;
		gameState = GameState.GAMEPLAY;
		levelStartTime = DateTime.Now;
	}
}
