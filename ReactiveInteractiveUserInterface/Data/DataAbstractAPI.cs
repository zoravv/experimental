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

namespace TP.ConcurrentProgramming.Data
{
    public enum DiagnosticEventType
    {
        PositionUpdate,
        WallBounce,
        CollisionDetected,
        CollisionNoBounce,
    }

    public class DiagnosticData
    {
        public DateTime Timestamp { get; set; }
        public int BallId { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public DiagnosticEventType EventType { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public abstract class DataAbstractAPI : IDisposable
    {
        #region Layer Factory

        public static DataAbstractAPI GetDataLayer()
        {
            return modelInstance.Value;
        }

        #endregion Layer Factory

        #region public API

        public abstract void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler);

        public abstract IVector CreateVector(double x, double y);

        public abstract void LogDiagnosticData(DiagnosticData data);

        #endregion public API

        #region IDisposable

        public abstract void Dispose();

        #endregion IDisposable

        #region private

        private static Lazy<DataAbstractAPI> modelInstance = new Lazy<DataAbstractAPI>(() => new DataImplementation());

        #endregion private
    }

    public interface IVector
    {
        /// <summary>
        /// The X component of the vector.
        /// </summary>
        double x { get; init; }

        /// <summary>
        /// The y component of the vector.
        /// </summary>
        double y { get; init; }
    }

    public interface IBall
    {
        event EventHandler<IVector> NewPositionNotification;
        int Id { get; }
        double Weight { get; }
        double BallRadius { get; }
        IVector Velocity { get; set; }
    }
}