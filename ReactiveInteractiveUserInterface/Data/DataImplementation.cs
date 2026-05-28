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

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation() 
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string solutionRoot = Path.GetFullPath(
                Path.Combine(baseDirectory, "..", "..", "..", ".."));
            
            string logDirectory = Path.Combine(solutionRoot, "Logs");
            string logFilePath = Path.Combine(logDirectory, "diagnostic.log");
            _logger = new Logger(logFilePath);
        }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Random random = new Random();
            const double minDistance = 25;
            List<IVector> existingPositions = new List<IVector>();

            for (int i = 0; i < numberOfBalls; i++)
            {
                Vector startingPosition = null;
                bool validPosition = false;
                int maxAttempts = 1000;
                for (int attempt = 0; attempt < maxAttempts && !validPosition; attempt++)
                {
                    startingPosition = new Vector(random.Next(100, 300), random.Next(100, 380));
                    validPosition = true;
                    foreach (var pos in existingPositions)
                    {
                        double distance = Math.Sqrt(Math.Pow(startingPosition.x - pos.x, 2) + Math.Pow(startingPosition.y - pos.y, 2));
                        if (distance < minDistance)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                }
                existingPositions.Add(startingPosition);

                Vector startingVelocity = new(random.Next(-150, 150), random.Next(-150, 150));
                double weight = 1.0;
                Ball newBall = new(startingPosition, startingVelocity, weight, _logger);
                upperLayerHandler(startingPosition, newBall);
                lock (_balllock)
                {
                    _ballTasks.Add(newBall.GetMovementTask());
                    _ballsList.Add(newBall);
                }
            }
        }

        public override IVector CreateVector(double x, double y)
        {
            return new Vector(x, y);
        }

        public override void LogDiagnosticData(DiagnosticData data)
        {
            _logger.Log(data);
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));

            if (disposing)
            {
                lock (_balllock)
                {
                    foreach (var ball in _ballsList)
                    {
                        ball.StopMovement();
                    }
                }
                try
                {
                    Task.WhenAll(_ballTasks).Wait();
                }
                catch (AggregateException ae)
                {
                    foreach (var inner in ae.InnerExceptions)
                    {
                        if (inner is not TaskCanceledException)
                        {
                            break;
                        }
                    }
                }

                _ballTasks.Clear();
                _ballsList.Clear();
                _logger.Dispose();
            }

            Disposed = true;
        }

        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private readonly List<Task> _ballTasks = new List<Task>();
        private readonly List<Ball> _ballsList = new List<Ball>();
        private readonly object _balllock = new();
        private readonly Logger _logger;

        //private bool disposedValue;
        private bool Disposed = false;

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(_ballsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(_ballsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}