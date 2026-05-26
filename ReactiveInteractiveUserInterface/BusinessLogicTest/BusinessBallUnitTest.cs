//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void MoveTestMethod()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            DataLayerFixture dataLayerFixture = new DataLayerFixture();
            Ball newInstance = new(dataBallFixture, dataLayerFixture, new PositionFixture(0.0, 0.0));
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); Assert.IsNotNull(position); numberOfCallBackCalled++; };
            dataBallFixture.Move();
            Assert.AreEqual<int>(1, numberOfCallBackCalled);
        }

        #region testing instrumentation

        private class DataBallFixture : Data.IBall
        {
            private Data.IVector _velocity = new VectorFixture(0.0, 0.0);

            public Data.IVector Velocity
            {
                get => _velocity;
                set => _velocity = value;
            }

            public double Weight { get; } = 1.0;

            public double BallRadius { get; } = 10.0;

            public int Id => 0;

            public event EventHandler<Data.IVector>? NewPositionNotification;

            internal void Move()
            {
                NewPositionNotification?.Invoke(this, new VectorFixture(5.0, 5.0));
            }
        }

        private class DataLayerFixture : Data.DataAbstractAPI
        {
            public override void Dispose() { }

            public override void Start(int numberOfBalls, Action<Data.IVector, Data.IBall> upperLayerHandler)
            {
                throw new NotImplementedException();
            }

            public override Data.IVector CreateVector(double x, double y)
            {
                return new VectorFixture(x, y);
            }

            public override void LogDiagnosticData(Data.DiagnosticData data)
            {
                throw new NotImplementedException();
            }
        }

        private record PositionFixture(double x, double y) : IPosition;

        private class VectorFixture : Data.IVector
        {
            internal VectorFixture(double X, double Y)
            {
                x = X; y = Y;
            }

            public double x { get; init; }
            public double y { get; init; }
        }

        #endregion testing instrumentation
    }
}
