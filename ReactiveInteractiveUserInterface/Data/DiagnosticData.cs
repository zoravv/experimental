using System;
using System.Text.Json;

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
        public string Message { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
