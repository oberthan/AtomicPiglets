using System;
using GameLogic;
using UnityEngine;

namespace Assets.Network
{
    public class GameServerTimer : IPlayTimer
    {
        private readonly GameServer _gameServer;
        private float _elapseTime;
        private float _startTime;

        public GameServerTimer(GameServer gameServer)
        {
            _gameServer = gameServer;
        }

        public void Update()
        {
            if (_elapseTime == 0) return;

            var time = Time.time;
            var timeLeft = _elapseTime - time;
            if (timeLeft < 0) timeLeft = 0;
            _gameServer.ExecutePlayedCardsTimer = timeLeft;
            if (timeLeft <= 0)
            {
                Debug.Log("Game timer elapsed");
                OnTimerElapsed();
                _elapseTime = 0;
                _gameServer.UpdateClients();
            }
        }

        public void Start(float delay)
        {
            Debug.Log($"Game timer started with {delay} delay");
            _startTime = Time.time;
            _elapseTime = _startTime + delay;
            _gameServer.ExecutePlayedCardsTimerMax = delay;
            if (delay <= 0) OnTimerElapsed();
        }

        public event EventHandler TimerElapsed;

        private void OnTimerElapsed()
        {
            TimerElapsed?.Invoke(this, EventArgs.Empty);
        }
    }
}