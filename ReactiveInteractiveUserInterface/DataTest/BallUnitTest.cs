//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Reflection;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class BallUnitTest
    {
        private Logger _testLogger;

        [TestCleanup]
        public void TestCleanup()
        {
            _testLogger.Dispose();
        }

        [TestMethod]
        public void ConstructorTestMethod()
        {
            string tempLogFilePath = Path.Combine(Path.GetTempPath(), $"diagnostic_log_test_{Guid.NewGuid()}.txt");
            _testLogger = new Logger(tempLogFilePath);
            Vector testinVector = new Vector(0.0, 0.0);
            double weight = 1.0;
            Ball newInstance = new(testinVector, testinVector, weight, _testLogger);
        }

        [TestMethod]
        public void MoveTestMethod()
        {
            string tempLogFilePath = Path.Combine(Path.GetTempPath(), $"diagnostic_log_test_{Guid.NewGuid()}.txt");
            _testLogger = new Logger(tempLogFilePath);
            Vector initialPosition = new(10.0, 10.0);
            Vector delta = new(0.0, 0.0); 
            double weight = 1.0;
            Ball newInstance = new(initialPosition, new Vector(0.0, 0.0), weight, _testLogger);
            IVector curentPosition = new Vector(0.0, 0.0);
            int numberOfCallBackCalled = 0;

            newInstance.NewPositionNotification += (sender, position) => {
                Assert.IsNotNull(sender);
                curentPosition = position;
                numberOfCallBackCalled++;
            };

            Type ballType = typeof(Ball);
            MethodInfo? moveMethod = ballType.GetMethod("Move", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Vector) }, null);
            Assert.IsNotNull(moveMethod, "Private method 'Move' was not found.");
            moveMethod.Invoke(newInstance, new object[] { delta });

            Assert.AreEqual<int>(1, numberOfCallBackCalled);
            Assert.AreEqual<IVector>(initialPosition, curentPosition);
        }
    }
}
