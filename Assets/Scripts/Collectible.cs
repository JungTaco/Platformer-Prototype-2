using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class Collectible : MonoBehaviour
{
	public static Action<int> OnCollectedTest;
	protected int _rotateSpeed;
	private AudioSource collectSound;
	protected int _score;

	private void OnTriggerEnter(Collider collider)
	{
		if(collider.gameObject.tag == "Player")
		{
			Destroy(gameObject);
			collectSound.Play();
			OnCollectedTest?.Invoke(_score);
		}	
	}

	private void Awake()
	{
		collectSound = GetComponent<AudioSource>();
	}

	void Update()
	{
		transform.Rotate(0, _rotateSpeed * Time.deltaTime, 0, Space.World);
	}
}
