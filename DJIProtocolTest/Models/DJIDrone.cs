using DJIProtocolTest.Models.DJIProtocol;
using DJIProtocolTest.Models.Helpers;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DJIProtocolTest.Models
{
    public class DJIDrone : Drone
    {
        private DJICommand command;

        public int UID;

        // Connection
        public override string Param1 { get; set; }
        public override int Param2 { get; set; }
        public override int Timeout { get; set; } = 500;
        public override long RoundTripTime { get; set; } = 0;
        public override bool StopTcpClient { get; set; } = false;

        public override bool IsConnected { get; set; } = false;
        public override bool AutoReconnect { get; set; } = true;
        public override ConnectionType ConnType { get; set; }

        public override DroneStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                NotifyPropertyChanged(nameof(Status));
            }
        }

        public DJIDrone()
        {
            command = new DJICommand();            
        }

        /// <summary>Connect and set up drone for GCS control.</summary>
        public override void Connect()
        {
            StartConnection(this);
            InitiatePreSettings();
            InstantiateDataReceivedEventHandlers();
        }

        private void InstantiateDataReceivedEventHandlers()
        {
            if (ConnType == ConnectionType.SERIAL && serialPort != null)
            {
                serialPort.DataReceived += SerialPortDataReceived;
            }
            if (ConnType == ConnectionType.TCP && tcpClient != null)
            {
                TcpClientDataReceived();                
            }
        }

        public override ConnectionType GetConnectionType()
        {
            return this.ConnType;
        }

        public override void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override async void TcpClientDataReceived()
        {
            int counter = 0;
            int pingTimer = 0;

            try
            {
                await Task<int>.Run(() =>
                {
                    while (!StopTcpClient)
                    {
                        if (tcpClient != null && tcpClient.Connected == true && tcpClient.Client != null)
                        {
                            if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                            {
                                NetworkStream stream = tcpClient.GetStream();

                                while (stream.DataAvailable == true)
                                {
                                    if (pingTimer++ >= 100)
                                    {
                                        RoundTripTime = (int)TcpPing(Param1, 500).RoundtripTime;
                                        pingTimer = 0;
                                    }

                                    if (StopTcpClient)
                                    {
                                        break;
                                    }

                                    int bytesToRead = tcpClient.Available;

                                    IsConnected = true;

                                    byte[] dataBytes = new byte[bytesToRead];
                                    stream.Read(dataBytes, 0, dataBytes.Length);

                                    // Process incoming bytes here
                                    //drone._telemetry.ProcessPacketReceived(dataBytes);
                                }
                            }
                            else
                            {
                                if (AutoReconnect == true)
                                {
                                    counter++;

                                    // Attempt reconnect as incoming stream lost
                                    if (counter >= 10)
                                    {
                                        // If client instantiated, free up resources
                                        if (tcpClient.Client != null && tcpClient != null)
                                        {
                                            tcpClient.Close();
                                        }

                                        StartConnection(ConnectionType.TCP, Param1, Param2, Timeout);
                                        byte[] temp_data = new byte[] { 0xaa, 0x19, 0x00, 0x00 };
                                        SendDataStream(temp_data);
                                        InitiatePreSettings();

                                        counter = 0;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Attempt reconnection as client not connected
                            StartConnection(ConnectionType.TCP, Param1, Param2, Timeout);
                        }

                        Thread.Sleep(100);
                    }
                });
            }
            catch (Exception e)
            {
                MyDebug.WriteLine($"StartTCPClientReceive: {e.Message}");
            }
        }

        #region Drone Commands

        public override bool Arm()
        {
            SendDataStream(command.Arm());
            //Debug.WriteLine($"{this.GetType().Name} ARMED");
            return true;
        }
        
        public override bool Disarm()
        {
            SendDataStream(command.Disarm());
            //Debug.WriteLine($"{this.GetType().Name} DISARMED");
            return true;
        }

        public override bool InitiatePreSettings()
        {
            SendDataStream(command.Activation());

            byte[] obtainControlBytes= command.ObtainControl();
            for (int i = 0; i < 2; i++)
            {
                SendDataStream(obtainControlBytes);
            }

            SendDataStream(command.SetFlightDataFrequency(true));
            return true;
        }

        public override bool Launch()
        {
            SendDataStream(command.SwitchFlightMode(DJICommand.FlightMode.AutoTakeOff));
            return true;
        }

        public override bool Land()
        {
            SendDataStream(command.SwitchFlightMode(DJICommand.FlightMode.AutoLanding));
            return true;
        }

        public override bool ReturnToHome()
        {
            SendDataStream(command.SwitchFlightMode(DJICommand.FlightMode.ReturnToHome));
            return true;
        }

        public override bool UploadWaypointMission()
        {
            // Number of retries permitted
            int retries = 3;

            if (Status.mission == null)
            {
                // No mission data
                MessageBox.Show("No waypoint mission to upload.");
                return false;
            }
            else if (!(status.mission is DJIWaypointMission))
            {
                // Not waypoint mission
                MessageBox.Show("Saved mission is not a waypoint mission.");
                return false;
            }
            else
            {
                while (retries > 0)
                {
                    // Send Waypoint Mission Init CMD
                    SendDataStream(command.UploadWaypointMissionSettings((DJIWaypointMission)status.mission));

                    if (AckLock.WaitOne(300))
                    {
                        // Waypoint Mission Init Ack Received
                        // Allow Mutex
                        AckLock.Set();

                        // Send individual Waypoint Data 
                        for (int i = 0; i < (status.mission as DJIWaypointMission).GetWaypointCount(); i++)
                        {
                            // Wait for Ack from previous command
                            if (AckLock.WaitOne(300))
                            {
                                SendDataStream(command.UploadWaypointData((status.mission as DJIWaypointMission), i));
                            }
                            else
                            {
                                // Timed out waiting for Ack
                                MyDebug.ConsoleWriteLine($"Upload Waypoint Data: WP {i} Ack Timed Out");
                                
                                if (retries-- > 0)
                                {
                                    // Reattempt to upload waypoint
                                    i--;
                                    AckLock.Set();
                                }
                                else
                                {
                                    return false;
                                }                                
                            }
                        }

                        // Wait for Ack for last waypoint uploaded
                        if (AckLock.WaitOne(300))
                        {
                            // All Waypoints uploaded successfully
                            MyDebug.ConsoleWriteLine("Waypoint Mission Upload Complete.");
                            return true;
                        }
                        else
                        {
                            // Last waypoint failed to upload
                            retries--;
                        }
                    }
                    else
                    {
                        // Waypoint Mission Init failed to upload
                        retries--;
                    }
                }
            }
            return false;
        }

        public override bool StartWaypointMission()
        {
            SendDataStream(command.StartWaypointMission());
            return true;
        }

        public override bool NudgeInDirection(float heading, double distance)
        {
            DJIWaypointMission tempNudgeMission = new DJIWaypointMission();

            DJIWaypoint tempNudgeWaypoint = new DJIWaypoint(0,0,0);

            throw new NotImplementedException();
        }

        public override bool StopWaypointMission()
        {
            SendDataStream(command.StopWaypointMission());
            return true;
        }

        public override bool DownloadWaypointMission()
        {
            // Number of retries permitted
            int retries = 3;

            while (retries > 0)
            {
                SendDataStream(command.GetWaypointInitStatus());

                if (AckLock.WaitOne(300))
                {
                    if ((status.mission as DJIWaypointMission).GetWaypointCount() >= 0)
                    {
                        // Obtained Waypoint mission init status
                        // Allow Mutex
                        AckLock.Set();

                        // Download individual waypoints
                        for (int i = 0; i < (status.mission as DJIWaypointMission).GetWaypointCount(); i++)
                        {
                            if (AckLock.WaitOne(300))
                            {
                                SendDataStream(command.GetSingleWaypointStatus((byte)i));
                            }
                            else
                            {
                                // Timed out waiting for Ack
                                MyDebug.ConsoleWriteLine($"Download Single Waypoint: WP {i} Ack Timed Out");

                                if (retries-- > 0)
                                {
                                    // Reattempt to upload waypoint
                                    i--;
                                    AckLock.Set();
                                }
                                else
                                {
                                    // Download Waypoint Mission failed. Exceeded number of retries.
                                    return false;
                                }
                            }
                        }

                        // Wait for ack for final waypoint
                        if (AckLock.WaitOne(300))
                        {
                            // Waypoint mission download complete
                            MyDebug.ConsoleWriteLine("Waypoint Mission download complete.");
                            return true;
                        }
                        else
                        {
                            // Last waypoint failed to download
                            retries--;
                        }
                    }
                    else
                    {
                        // Invalid number of waypoints in waypoint mission
                        retries--;
                    }
                }
                else
                {
                    // Get Waypoint Init status Ack time out
                    retries--;
                }

            }
            return false;
        }

        public override bool StartFollowMeMission()
        {
            if (Status.mission == null)
            {
                // No mission data
                return false;
            }
            else if (!(status.mission is DJIWaypointMission))
            {
                // Not waypoint mission
                return false;
            }
            SendDataStream(command.UploadAndStartFollowMe(status.mission as DJIFollowMeMission));
            return true;
        }

        public override bool StopFollowMeMission()
        {
            SendDataStream(command.StopFollowMe());
            return true;
        }

        public override bool HighLaunch()
        {
            DJIWaypointMission tempHighLaunchMission = new DJIWaypointMission();

            for (int i = 0; i < 3; i++)
            {
                DJIWaypoint tempHighLaunchWaypoint = new DJIWaypoint(
                    status.latitude,
                    status.longitude,
                    status.height + ((i + 0) * 5)
                    );
                tempHighLaunchMission.AddWaypoint(tempHighLaunchWaypoint);
            }

            SendDataStream(command.UploadWaypointMissionSettings(tempHighLaunchMission));

            Thread.Sleep(100);

            for (int i = 0; i < tempHighLaunchMission.GetWaypointCount(); i++)
            {
                SendDataStream(command.UploadWaypointData(tempHighLaunchMission, i));

                Thread.Sleep(100);
            }

            SendDataStream(command.StartWaypointMission());
            return true;
        }

        public override bool FlyToHere()
        {
            return true;
        }

        #endregion Drone Commands
    }
}