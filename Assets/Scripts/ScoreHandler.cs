using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
	public static Action<int> OnScoreChanged;

	private static int _score;
	private List<int> _scoreList;

	public static int Score { get { return _score; } }

	private void OnEnable()
	{
		Collectible.OnCollected += PointCollected;
		// ...+=SaveScoretoList;
	}

	private void OnDisable()
	{
		Collectible.OnCollected -= PointCollected;
		// ...-=SaveScoretoList;
	}

	void Start()
    {
		DontDestroyOnLoad(this);
		_score = 0;
		_scoreList = new List<int>();
	}

    private void PointCollected(int receivedScore)
	{
		_score += receivedScore;
		OnScoreChanged?.Invoke(_score);
	}

	private void SaveScoretoList()
	{
		_scoreList.Add(_score);
		_score = 0;
	}
}
