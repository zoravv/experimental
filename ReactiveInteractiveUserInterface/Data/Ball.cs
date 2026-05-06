//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity, double weight)
        {
            _position = initialPosition;
            Velocity = initialVelocity;
            BallRadius = 10;
            Weight = weight;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }

        public IVector Position => _position;

        public double Weight { get; }

        public double BallRadius { get; }

        public void StopMovement()
        {
            _cancellationTokenSource.Cancel();
        }

        #endregion IBall

        #region private

        private readonly object _velocityLock = new object();

        private readonly CancellationTokenSource _cancellationTokenSource;

        private Task? _movementTask;

        private Vector _position;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        private void Move(Vector delta)
        {
            _position = new Vector(_position.x + delta.x, _position.y + delta.y);
            RaiseNewPositionChangeNotification();
        }

        private async Task RunMovementLoop(CancellationToken token)
        {
            const int movementIntervalMs = 16;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(movementIntervalMs, token);

                    IVector currentVelocity;
                    lock (_velocityLock)
                    {
                        currentVelocity = Velocity;
                    }

                    Vector velocityVector = (Vector)currentVelocity;
                    Vector delta = velocityVector * (movementIntervalMs / 1000.0);

                    Move(delta);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    break;
                }
            }
        }

        internal Task StartMovementTask()
        {
            _movementTask = Task.Run(() => RunMovementLoop(_cancellationTokenSource.Token));
            return _movementTask;
        }

        #endregion private
    }
}