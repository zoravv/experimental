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
using System.ComponentModel.DataAnnotations;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class Ball : IBall
    {
        private readonly Data.IBall _underlyingBall;
        private readonly Data.DataAbstractAPI _dataLayer;
        private IPosition _currentPosition;
        private readonly object _positionLock = new object();
        private readonly double tableWidth = 400;
        private readonly double tableHeight = 400;

        internal Ball(Data.IBall ball, Data.DataAbstractAPI dataLayer, IPosition initialPosition)
        {
            _underlyingBall = ball;
            _dataLayer = dataLayer;
            _currentPosition = initialPosition;
            _underlyingBall.NewPositionNotification += RaisePositionChangeEvent;
        }

        #region IBall

        public event EventHandler<IPosition>? NewPositionNotification;

        public IPosition GetPosition()
        {
            lock (_positionLock)
            {
                return _currentPosition;
            }
        }

        public Data.IVector GetVelocity()
        {
            return _underlyingBall.Velocity;
        }

        public double GetWeight()
        {
            return _underlyingBall.Weight;
        }

        public int getId()
        {
            return _underlyingBall.Id;
        }

        public double GetBallRadius()
        {
            return _underlyingBall.BallRadius;
        }

        public void SetVelocity(Data.IVector velocity)
        {
            _underlyingBall.Velocity = velocity;
        }

        #endregion IBall

        #region private

        private void RaisePositionChangeEvent(object? sender, Data.IVector dataPosition)
        {
            lock (_positionLock)
            {
                _currentPosition = new Position(dataPosition.x, dataPosition.y);
            }

            IPosition pos1 = GetPosition();
            Data.IVector v1 = GetVelocity();
            int id = getId();
            double currentVx = v1.x;
            double currentVy = v1.y;
            double newVx = currentVx;
            double newVy = currentVy;
            double positionX = pos1.x;
            double positionY = pos1.y;
            double radius = GetBallRadius();

            if (positionX <= radius && currentVx < 0)
            {
                newVx = -currentVx;
            }
            else if (positionX >= tableWidth - radius && currentVx > 0)
            {
                newVx = -currentVx;
            }

            if (positionY <= radius && currentVy < 0)
            {
                newVy = -currentVy;
            }
            else if (positionY >= tableHeight - radius && currentVy > 0)
            {
                newVy = -currentVy;
            }

            if (newVx != currentVx || newVy != currentVy)
            {
                SetVelocity(_dataLayer.CreateVector(newVx, newVy));
                v1 = GetVelocity();
                currentVx = v1.x;
                currentVy = v1.y;
                _dataLayer.LogDiagnosticData(new DiagnosticData
                {
                    Timestamp = DateTime.Now,
                    BallId = id,
                    PositionX = pos1.x,
                    PositionY = pos1.y,
                    VelocityX = newVx,
                    VelocityY = newVy,
                    EventType = DiagnosticEventType.WallBounce,
                    Message = $"Wall bounce detected."
                });
            }
            NewPositionNotification?.Invoke(this, GetPosition());
        }

        internal void DetachFromDataBall()
        {
            if (_underlyingBall != null)
            {
                _underlyingBall.NewPositionNotification -= RaisePositionChangeEvent;
            }
        }

        #endregion private
    }
}