using System;
using UnityEngine;

public abstract class Collectible : MonoBehaviour
{
	public static Action<int> OnCollected;
	protected int _rotateSpeed;
	protected int _score;
	private GameObject _camera;
	
	[SerializeField]
	protected AudioClip _clip;

	private void OnTriggerEnter(Collider collider)
	{
		if(collider.gameObject.tag == "Player")
		{
			AudioSource.PlayClipAtPoint(_clip, _camera.transform.position, 1);
			Destroy(gameObject);
			OnCollected?.Invoke(_score);
		}	
	}

	protected void Init()
	{
		_camera = GameObject.FindGameObjectWithTag("MainCamera");
	}

	void Update()
	{
		transform.Rotate(0, _rotateSpeed * Time.deltaTime, 0, Space.World);
	}
}
