﻿using System.Collections;
using Borodar.LD34.Helpers;
using Borodar.LD34.Questions;
using UnityEngine;
using UnityEngine.UI;

namespace Borodar.LD34.Managers
{
    public class SceneManager : Singleton<SceneManager>
    {
        private const float TIME_PER_QUESTION = 6f;

        [Space(10)]
        public Background Background;
        public Text TimeText;
        public Text ScoreText;
        public Text HighscoreText;
        public Text QuestionText;
        public Text HintText;
        [Space(10)]
        public ParticleSystem YesParticles;
        public ParticleSystem NoParticles;
        [Space(10)]
        public Color CorrectColor;
        public Color WrongColor;

        private Question _question;
        private bool _isFirstQuestion = true;
        private bool _isQuestionTrue = true;
        private bool _isCheckingAnswer;
        private int _score;
        private float _timeRemaining;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        public void Start()
        {
            var game = GlobalManager.Game;
            if (game.IsFirstRun)
            {
                game.IsFirstRun = false;
            }
            else
            {
                QuestionText.text = "Play again?";
            }

            UpdateHighscore();
        }

        protected void Update()
        {
            if (_isFirstQuestion || _isCheckingAnswer) return;            

            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining < 0) _timeRemaining = 0;

            var seconds = _timeRemaining % 60;
            var centiseconds = Mathf.Floor(_timeRemaining * 100) % 100;

            TimeText.gameObject.SetActive(true);
            TimeText.text = string.Format("{0:00} : {1:00}", seconds, centiseconds);

            if (_timeRemaining <= 0 && !_isCheckingAnswer) StartCoroutine(GameOver());
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void GenerateQuestion()
        {
            _question = new Question();
            _isQuestionTrue = Random.value > 0.5f;

            QuestionText.text = (_isQuestionTrue) ? _question.GetTrueString() : _question.GetFakeString();
        }

        public void CheckAnswer(bool answer)
        {
            if (_isCheckingAnswer) return;

            if (_isFirstQuestion && !answer)
            {
                Application.Quit();
                return; // for web-player
            }

            var isAnswerCorrect = (answer == _isQuestionTrue);
            if (isAnswerCorrect)
            {
                GlobalManager.Audio.PlayRandomCorrectSound();
                UpdateScore();

                if (_isQuestionTrue)
                {
                    YesParticles.Play();
                }
                else
                {
                    NoParticles.Play();
                }

                StartCoroutine(ShowNextQuestion());
            }
            else
            {
                StartCoroutine(GameOver());
            }

            Background.CrossFadeColor();
            _isFirstQuestion = false;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateScore()
        {
            if (_isFirstQuestion) return;

            _score++;

            ScoreText.text = "Score: " + _score.ToString("000");
            ScoreText.gameObject.SetActive(true);
        }

        private void UpdateHighscore()
        {
            var game = GlobalManager.Game;
            if (game.HighScore < _score) game.HighScore = _score;
            if (game.HighScore <= 0) return;

            HighscoreText.text = "Highscore: " + game.HighScore.ToString("000");
            HighscoreText.gameObject.SetActive(true);
        }

        private IEnumerator ShowNextQuestion()
        {
            _isCheckingAnswer = true;
            yield return new WaitForSeconds(1f);

            HintText.gameObject.SetActive(false);
            GenerateQuestion();

            _isCheckingAnswer = false;
            _timeRemaining = TIME_PER_QUESTION;
        }

        private IEnumerator GameOver()
        {
            _isCheckingAnswer = true;

            GlobalManager.Audio.StopMusic();
            GlobalManager.Audio.PlayWrongSound();

            UpdateHighscore();

            if (_isQuestionTrue)
            {
                QuestionText.color = CorrectColor;
            }
            else
            {
                QuestionText.color = WrongColor;
                //HintText.text = "Highscore: " + game.HighScore.ToString("000");
                HintText.text = _question.GetTrueString();
                HintText.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(4f);

            GlobalManager.Game.LoadScene(Application.loadedLevelName);
        }
    }
}