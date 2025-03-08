﻿using DrawnUi.Maui.Draw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawnUi.Maui.Game
{

    public interface IMauiGame
    {
        void OnKeyDown(MauiKey key);
        void OnKeyUp(MauiKey key);

        void Pause();
        void Resume();
        void StopLoop();
        void StartLoop(int delayMs = 0);
    }

    /// <summary>
    /// Base class for implementing a game. StartLoop, StopLoop, override GameLoop(..) etc.
    /// </summary>
    public class MauiGame : SkiaLayout, IMauiGame
    {

        private ActionOnTickAnimator _appLoop;
        protected long LastFrameTimeNanos;

        public MauiGame()
        {
            KeyboardManager.KeyDown += OnKeyboardDownEvent;
            KeyboardManager.KeyUp += OnKeyboardUpEvent;
        }

        public override void OnDisposing()
        {
            KeyboardManager.KeyUp -= OnKeyboardUpEvent;
            KeyboardManager.KeyDown -= OnKeyboardDownEvent;

            base.OnDisposing();
        }

        protected virtual void OnResumed()
        {
        }

        protected virtual void OnPaused()
        {
        }

        /// <summary>
        /// Override this for your game. `deltaMs` is time elapsed between the previous frame and this one 
        /// </summary>
        /// <param name="deltaMs"></param>
        public virtual void GameLoop(float deltaMs)
        {

        }

        /// <summary>
        /// Stops game loop
        /// </summary>
        public virtual void StopLoop()
        {
            _appLoop.Stop();
        }

        /// <summary>
        /// Starts game loop
        /// </summary>
        /// <param name="delayMs"></param>
        public void StartLoop(int delayMs = 0)
        {
            if (_appLoop == null)
            {
                _appLoop = new(this, GameTick);
            }
            _appLoop.Start(delayMs);
        }

        /// <summary>
        /// Internal, use override GameLoop for your game.
        /// </summary>
        /// <param name="frameTime"></param>
        protected virtual void GameTick(long frameTime)
        {
            // Incoming frameTime is in nanoseconds
            // Calculate delta time in seconds for later use
            float deltaTime = (frameTime - LastFrameTimeNanos) / 1_000_000_000.0f;
            LastFrameTimeNanos = frameTime;

            GameLoop(deltaTime);
        }

        private bool _IsPaused;
        public bool IsPaused
        {
            get
            {
                return _IsPaused;
            }
            set
            {
                if (_IsPaused != value)
                {
                    _IsPaused = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Pause()
        {
            IsPaused = true;
            OnPaused();
        }

        public void Resume()
        {
            LastFrameTimeNanos = SkiaControl.GetNanoseconds();
            IsPaused = false;
            OnResumed();
        }

        #region KEYS

        /// <summary>
        /// Override this to process game keys
        /// </summary>
        /// <param name="key"></param>
        public virtual void OnKeyDown(MauiKey key)
        {

        }

        /// <summary>
        /// Override this to process game keys
        /// </summary>
        /// <param name="key"></param>
        public virtual void OnKeyUp(MauiKey key)
        {

        }


        /// <summary>
        /// Do not use directly. It's public to be able to send keys to game manually if needed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="key"></param>
        public void OnKeyboardDownEvent(object sender, MauiKey key)
        {
            OnKeyDown(key);
        }

        /// <summary>
        /// Do not use directly. It's public to be able to send keys to game manually if needed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="key"></param>
        public void OnKeyboardUpEvent(object sender, MauiKey key)
        {
            OnKeyUp(key);
        }


        #endregion

    }
}