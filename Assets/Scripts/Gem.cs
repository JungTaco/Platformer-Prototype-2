using UnityEngine;

public class Gem : Collectible
{
	private void Awake()
	{
		_rotateSpeed = 50;
		_score = 5;
	}
}
