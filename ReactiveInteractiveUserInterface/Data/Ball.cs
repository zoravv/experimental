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
using System.Diagnostics;
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
            Weight = weight;
            BallRadius = CalculateRadiusFromWeight(weight);
            _cancellationTokenSource = new CancellationTokenSource();
            _movementTask = Task.Run(() => RunMovementLoop(_cancellationTokenSource.Token));
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

        private readonly object _positionLock = new object();

        private readonly CancellationTokenSource _cancellationTokenSource;

        private Task? _movementTask;

        private Vector _position;

        private static double CalculateRadiusFromWeight(double weight)
        {
            const double minRadius = 5.0;    
            const double maxRadius = 15.0;     
            const double minWeight = 1.0;
            const double maxWeight = 5.0;

            double normalized = (weight - minWeight) / (maxWeight - minWeight);
            return minRadius + (normalized * (maxRadius - minRadius));
        }

        private int CalculateMovementIntervalMs(IVector velocity)
        {
            double velocityMagnitude = Math.Sqrt(velocity.x * velocity.x + velocity.y * velocity.y);

            const int minIntervalMs = 8;
            const int maxIntervalMs = 24;
            const double velocityThreshold = 200.0;

            if (velocityMagnitude <= 0)
                return maxIntervalMs;

            double normalizedVelocity = Math.Min(velocityMagnitude / velocityThreshold, 1.0);
            int calculatedInterval = (int)(maxIntervalMs - (normalizedVelocity * (maxIntervalMs - minIntervalMs)));

            return Math.Max(minIntervalMs, Math.Min(maxIntervalMs, calculatedInterval));
        }

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        private void Move(Vector delta)
        {
            lock (_positionLock)
            {
                _position = new Vector(_position.x + delta.x, _position.y + delta.y);
            }
            RaiseNewPositionChangeNotification();
        }

        private async Task RunMovementLoop(CancellationToken token)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    IVector currentVelocity;
                    lock (_velocityLock)
                    {
                        currentVelocity = Velocity;
                    }
                    int movementIntervalMs = CalculateMovementIntervalMs(currentVelocity);

                    await Task.Delay(movementIntervalMs, token);
                    double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
                    stopwatch.Restart();
                    Vector velocityVector = (Vector)currentVelocity;
                    Vector delta = velocityVector * (elapsedMs / 1000.0);

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

        internal Task GetMovementTask()
        {
            return _movementTask ?? Task.CompletedTask;
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void MoveForTest(Vector delta) { Move(delta); }

        #endregion TestingInfrastructure
    }
}