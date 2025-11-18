using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
	private int score;
	private List<int> scoreList;

	private void OnEnable()
	{
		Collectible.OnCollectedTest += PointCollected;
		// ...+=SaveScoretoList;
	}

	private void OnDisable()
	{
		Collectible.OnCollectedTest -= PointCollected;
		// ...-=SaveScoretoList;
	}

	void Start()
    {
		DontDestroyOnLoad(this);
		score = 0;
		scoreList = new List<int>();
	}

    private void PointCollected(int receivedScore)
	{
		score += receivedScore;
		Debug.Log(score);
	}

	private void SaveScoretoList()
	{
		scoreList.Add(score);
		score = 0;
	}
}
