using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

	public static SoundManager Instance { get; private set; }

	[SerializeField] private AudioClip music;
	[SerializeField] private AudioClip playerAttack;
	[SerializeField] private AudioClip playerHit;
	[SerializeField] private AudioClip victory;
	[SerializeField] private AudioClip gameOver;
	[SerializeField] private AudioClip enemyKill;


	public enum SoundType {
		PlayerMove,
		PlayerAttack,
		PlayerHit,

		EnemyKill,
		GameWin,
		GameOver
	}

	public void playSound(SoundType type) {
		AudioSource audioSource = this.gameObject.AddComponent<AudioSource>();
		switch (type) {
			case SoundType.PlayerAttack:
				audioSource.PlayOneShot(playerAttack);
				break;
			case SoundType.PlayerHit:
				audioSource.PlayOneShot(playerHit);
				break;
			case SoundType.GameWin:
				audioSource.PlayOneShot(victory);
				break;
			case SoundType.GameOver:
				audioSource.PlayOneShot(gameOver);
				break;
			case SoundType.EnemyKill:
				audioSource.PlayOneShot(enemyKill);
				break;
		}
	}

	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(this.gameObject);
		} else {
			Instance = this;
		}
		DontDestroyOnLoad(this);
	}

	private void Start() {
		// play music
		AudioSource audioSource = this.gameObject.AddComponent<AudioSource>();
		audioSource.clip = music;
		audioSource.loop = true;
		audioSource.volume = 0.3f;
		audioSource.Play();

	}
}
