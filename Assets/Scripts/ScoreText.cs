using System;
using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    //[SerializeField]
    //private ScoreHandler _scoreHandler;
	private TextMeshProUGUI _scoreText;

	private void OnEnable()
	{
		ScoreHandler.OnScoreChanged += UpdateText;
	}

	private void OnDisable()
	{
		ScoreHandler.OnScoreChanged -= UpdateText;
	}

	void Start()
    {
		_scoreText = GetComponent<TextMeshProUGUI>();
		_scoreText.text = ScoreHandler.Score.ToString();
	}

	private void UpdateText(int score)
	{
		_scoreText.text = score.ToString();
	}
}
