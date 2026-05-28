using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    public class Logger : IDisposable
    {
        private readonly DiagnosticBuffer _buffer;
        private readonly string _logFilePath;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task? _loggingTask;
        private readonly object _lock = new object();
        private bool _disposed = false;
        private int _overflowCount = 0; // licznik odrzuconych wpisów — inkrementowany przez wątki kulek, odczytywany przez Task loggera

        public Logger(string logFilePath, int bufferCapacity = 1000)
        {
            _logFilePath = logFilePath;
            string? logDirectory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                try
                {
                    Directory.CreateDirectory(logDirectory);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error creating log directory '{logDirectory}': {ex.Message}");
                }
            }
            _buffer = new DiagnosticBuffer(bufferCapacity);
            _cancellationTokenSource = new CancellationTokenSource();
            _loggingTask = Task.Run(() => ProcessLogQueue(_cancellationTokenSource.Token));
        }

        public void Log(DiagnosticData data)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    Console.WriteLine("Logger is disposed. Cannot log data.");
                    return;
                }

                if (!_buffer.TryAdd(data))
                {
                    // Bufor pełny — tylko inkrementujemy licznik, bez I/O na wątku kulki.
                    // Task loggera sam zapisze informację o przepełnieniu do pliku.
                    _overflowCount++;
                }
            }
        }

        private async Task ProcessLogQueue(CancellationToken token)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(_logFilePath, append: true))
                {
                    while (!token.IsCancellationRequested)
                    {
                        DiagnosticData? dataToLog = null;
                        int overflows;
                        lock (_lock)
                        {
                            _buffer.TryTake(out dataToLog);
                            // Odczytujemy i resetujemy licznik przepełnień pod tym samym lockiem
                            overflows = _overflowCount;
                            _overflowCount = 0;
                        }

                        // Jeśli były przepełnienia — Task loggera sam zapisuje info do pliku (nie wątek kulki!)
                        if (overflows > 0)
                        {
                            await sw.WriteLineAsync($"BUFFER_OVERFLOW: {overflows} diagnostic entries dropped at {DateTime.Now:O}");
                            await sw.FlushAsync();
                        }

                        if (dataToLog != null)
                        {
                            await sw.WriteLineAsync(dataToLog.ToJson());
                            await sw.FlushAsync();
                        }
                        else if (overflows == 0)
                        {
                            // Bufor pusty i brak przepełnień — czekaj na nowe dane
                            await Task.Delay(50, token);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            { }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in logging task: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Logging task finished.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Console.WriteLine("Disposing Logger...");
                    _cancellationTokenSource.Cancel();
                    try
                    {
                        _loggingTask?.Wait(); // Wait for the logging task to finish
                    }
                    catch (AggregateException ae)
                    {
                        foreach (var inner in ae.InnerExceptions)
                        {
                            if (inner is not TaskCanceledException)
                            {
                                Console.WriteLine($"Caught unexpected exception during Logger Dispose: {inner.GetType().Name}: {inner.Message}");
                            }
                        }
                    }
                    Console.WriteLine("Logger disposed.");
                }
                _disposed = true;
            }
        }
    }

    internal class DiagnosticBuffer
    {
        private readonly DiagnosticData?[] _buffer;
        private int _head = 0;
        private int _tail = 0;
        private int _count = 0;

        public DiagnosticBuffer(int capacity)
        {
            _buffer = new DiagnosticData[capacity];
        }

        public bool TryAdd(DiagnosticData data)
        {
            if (_count >= _buffer.Length)
                return false;

            _buffer[_tail] = data;
            _tail = (_tail + 1) % _buffer.Length;
            _count++;
            return true;
        }

        public bool TryTake(out DiagnosticData? data)
        {
            if (_count == 0)
            {
                data = null;
                return false;
            }

            data = _buffer[_head];
            _head = (_head + 1) % _buffer.Length;
            _count--;
            return true;
        }
    }
}