//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using System.Windows.Input;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        public double TableWidth => ModelLayer.TableWidth_scaled;
        public double TableHeight => ModelLayer.TableHeight_scaled;

        private double _windowWidth;
        public double WindowWidth
        {
            get => _windowWidth;
            set { _windowWidth = value; RaisePropertyChanged(nameof(WindowWidth)); }
        }

        private double _windowHeight;
        public double WindowHeight
        {
            get => _windowHeight;
            set { _windowHeight = value; RaisePropertyChanged(nameof(WindowHeight)); }
        }

        private double _uiScale = 1.0;
        public double UIScale
        {
            get => _uiScale;
            set { _uiScale = value; RaisePropertyChanged(nameof(UIScale)); }
        }

        #region ctor
        public MainWindowViewModel() : this(null) { }

        internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        {
            ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;

            double screenWidth = System.Windows.SystemParameters.WorkArea.Width;
            double screenHeight = System.Windows.SystemParameters.WorkArea.Height;

            WindowWidth = screenWidth * 0.8;
            WindowHeight = screenHeight * 0.8;

            UIScale = WindowHeight / 860.0;
            if (UIScale < 0.5) UIScale = 0.5;

            double clientHeight = WindowHeight - 60;
            double clientWidth = WindowWidth - 20;

            double availableLogicalWidth = clientWidth / UIScale;
            double availableLogicalHeight = clientHeight / UIScale;

            double logicalWidth = ModelLayer.GetDimensions.TableWidth;
            double logicalHeight = ModelLayer.GetDimensions.TableHeight;

            double baseMarginY = 220;
            double baseMarginX = 50;

            double scaleX = (availableLogicalWidth - baseMarginX) / logicalWidth;
            double scaleY = (availableLogicalHeight - baseMarginY) / logicalHeight;

            ModelAbstractApi.Scale = Math.Min(scaleX, scaleY);

            Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
            StartCommand = new RelayCommand(() => Start(BallCount), () => CanStart && BallCount > 0);
        }
        #endregion ctor

        #region public API

        private int _ballCount = 10;
        public int BallCount
        {
            get => _ballCount;
            set
            {
                if (_ballCount != value)
                {
                    _ballCount = value;
                    RaisePropertyChanged();
                    (StartCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _canStart = true;

        public bool CanStart
        {
            get => _canStart;
            set
            {
                if (_canStart != value)
                {
                    _canStart = value;
                    RaisePropertyChanged();
                }
            }
        }
        public ICommand StartCommand { get; }
        public void Start(int numberOfBalls)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            ModelLayer.Start(numberOfBalls);
            Observer.Dispose();

            CanStart = false;
        }

        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();
     
        #endregion public API

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Balls.Clear();
                    Observer.Dispose();
                    ModelLayer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                Disposed = true;
            }
        }

        public void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private IDisposable Observer = null;
        private ModelAbstractApi ModelLayer;
        private bool Disposed = false;

        #endregion private
    }
}