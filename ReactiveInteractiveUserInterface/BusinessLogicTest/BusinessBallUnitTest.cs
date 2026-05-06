//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void MoveTestMethod()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            var dataLayer = new MockDataLayer(); // Tworzymy instancję mocka dataLayer
            var initialPosition = new Position(0.0, 0.0);
            Ball newInstance = new(dataBallFixture, dataLayer, initialPosition);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => {
                Assert.IsNotNull(sender);
                Assert.IsNotNull(position);
                numberOfCallBackCalled++;
            };
            dataBallFixture.Move(); // Wywołanie metody, która uruchamia zdarzenie
            Assert.AreEqual<int>(1, numberOfCallBackCalled); // Sprawdzamy, czy callback został wywołany
        }

        #region testing instrumentation

        private class DataBallFixture : Data.IBall
        {
            private Data.IVector _velocity = new VectorFixture(1.0, 1.0); // przykładowe wartości

            public Data.IVector Velocity
            {
                get => _velocity;
                set => _velocity = value;
            }
            public double Weight => 1.0;
            public double BallRadius => 10.0;

            public event EventHandler<Data.IVector>? NewPositionNotification;

            internal void Move()
            {
                NewPositionNotification?.Invoke(this, new VectorFixture(0.0, 0.0));
            }
        }

        private class VectorFixture : Data.IVector
        {
            internal VectorFixture(double X, double Y)
            {
                x = X; y = Y;
            }

            public double x { get; init; }
            public double y { get; init; }
        }

        private class MockDataLayer : Data.DataAbstractAPI
        {
            public override void Dispose() { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler) { }

            public override IVector CreateVector(double x, double y)
            {
                return new MockVector(x, y);
            }

            private record MockVector(double x, double y) : IVector;
        }

        #endregion testing instrumentation
    }
}
