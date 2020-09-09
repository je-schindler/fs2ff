﻿// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Local

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using fs2ff.Annotations;
using fs2ff.FlightSim;

namespace fs2ff
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly FlightSimService _flightSim;

        private bool _errorOccurred;
        private IntPtr _hwnd = IntPtr.Zero;

        public MainViewModel(FlightSimService flightSim)
        {
            _flightSim = flightSim;
            _flightSim.StateChanged += FlightSim_StateChanged;

            ToggleConnectCommand = new ActionCommand(ToggleConnect, CanConnect);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private enum FlightSimState
        {
            Unknown,
            Connected,
            Disconnected,
            ErrorOccurred
        }

        public string ConnectButtonLabel => Connected ? "Disconnect" : "Connect";

        public Brush StateLabelColor =>
            CurrentFlightSimState switch
            {
                FlightSimState.Connected      => Brushes.Gold,
                FlightSimState.Disconnected   => Brushes.DarkGray,
                FlightSimState.ErrorOccurred  => Brushes.OrangeRed,
                _                             => Brushes.DarkGray
            };

        public string StateLabelText =>
            CurrentFlightSimState switch
            {
                FlightSimState.Connected      => "CONNECTED",
                FlightSimState.Disconnected   => "NOT CONNECTED",
                FlightSimState.ErrorOccurred  => "Unable to connect to Flight Simulator",
                _                             => ""
            };

        public ActionCommand ToggleConnectCommand { get; }

        private bool Connected => _flightSim.Connected;

        private FlightSimState CurrentFlightSimState =>
            _errorOccurred
                ? FlightSimState.ErrorOccurred
                : Connected
                    ? FlightSimState.Connected
                    : FlightSimState.Disconnected;

        public void Dispose()
        {
            _flightSim.StateChanged -= FlightSim_StateChanged;
            _flightSim.Dispose();
        }

        internal void SetWindowHandle(IntPtr hWnd)
        {
            _hwnd = hWnd;
            ToggleConnectCommand.TriggerCanExecuteChanged();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool CanConnect() => _hwnd != IntPtr.Zero;

        private void Connect() => _flightSim.Connect(_hwnd);

        private void Disconnect() => _flightSim.Disconnect();

        private void FlightSim_StateChanged(bool failure)
        {
            _errorOccurred = failure;
            OnPropertyChanged(nameof(StateLabelText));
            OnPropertyChanged(nameof(StateLabelColor));
            OnPropertyChanged(nameof(ConnectButtonLabel));
        }

        private void ToggleConnect()
        {
            if (Connected) Disconnect();
            else              Connect();
        }
    }
}
