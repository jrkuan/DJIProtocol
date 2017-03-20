using DJIProtocolTest.Models.DJIProtocol;
using DJIProtocolTest.Models.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DJIProtocolTest.Models.Drone;

namespace DJIProtocolTest.Models
{
    public abstract class Drone : DroneModel
    {
        /// <summary>Defines the type of connection with the Drone.</summary>
        public enum ConnectionType
        {
            SERIAL,
            TCP
        }

        /// <summary>Defines the type to protocol used to communicate with the Drone.</summary>
        public enum DroneProtocol
        {
            MAVLINK,
            DJILINK,
            HOPELINK
        }

        public abstract string Param1 { get; set; }
        public abstract int Param2 { get; set; }
        public abstract int Timeout { get; set; }
        public abstract long RoundTripTime { get; set; }
        public abstract bool StopTcpClient { get; set; }

        public abstract DroneStatus Status { get; set; } 

        /// <summary>Drone connection state.</summary>
        public abstract bool IsConnected { get; set; }

        /// <summary>Set whether drone should reconnect automatically on connection loss.</summary>
        public abstract bool AutoReconnect { get; set; }

        public abstract ConnectionType ConnType { get; set; }

        /// <summary>Gets the ConnectionType for this Drone.</summary>
        public abstract ConnectionType GetConnectionType();

        public abstract void Connect();

        public abstract bool InitiatePreSettings();

        public abstract bool Arm();

        public abstract bool Disarm();

        public abstract bool Launch();

        public abstract bool Land();

        public abstract bool ReturnToHome();

        public abstract bool UploadWaypointMission();

        public abstract bool StartWaypointMission();

        public abstract bool DownloadWaypointMission();

        public abstract bool NudgeInDirection(float heading, double distance);

        public abstract bool StopWaypointMission();

        public abstract bool StartFollowMeMission();

        public abstract bool StopFollowMeMission();

        public abstract bool HighLaunch();

        public abstract bool FlyToHere();

        public abstract void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e);

        public abstract void TcpClientDataReceived();

    }

    /// <summary>
    /// Base class for drones. Contains connection options and PropertyChanged notifications.
    /// </summary>
    public class DroneModel : INotifyPropertyChanged
    {
        public struct DroneStatus
        {
            public double latitude;
            public double longitude;
            public float altitude;
            public float height;
            public byte gpsHealth;
            public byte flightStatus;
            public float pitch;
            public float roll;
            public float yaw;
            public float velocity;
            public DJIMission mission;
        }

        public AutoResetEvent AckLock;

        public DroneModel()
        {
            status = new DroneStatus() { };
            AckLock = new AutoResetEvent(false);
        }

        public SerialPort serialPort;

        public TcpClient tcpClient;

        protected DroneStatus status;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Connect to either Serial or TCP. Parameters are hostname and port for TCP, and COM port and baudrate for Serial.</summary>
        protected bool StartConnection(ConnectionType connectionType, string param1, int param2, int timeout)
        {
            switch (connectionType)
            {
                case (ConnectionType.SERIAL):
                    tcpClient = null;
                    return SerialConnect(param1, param2, out serialPort);
                    
                case (ConnectionType.TCP):
                    serialPort = null;
                    return TcpConnect(param1, param2, timeout);

                default:
                    tcpClient = null;
                    serialPort = null;
                    return false;
            }            
        }

        /// <summary>Connect to either Serial or TCP. Parameters are hostname and port for TCP, and COM port and baudrate for Serial.</summary>
        protected bool StartConnection(Drone drone)
        {
            switch (drone.ConnType)
            {
                case (ConnectionType.SERIAL):
                    tcpClient = null;
                    return SerialConnect(drone.Param1, drone.Param2, out serialPort);

                case (ConnectionType.TCP):
                    serialPort = null;
                    return TcpConnect(drone.Param1, drone.Param2, drone.Timeout);

                default:
                    tcpClient = null;
                    serialPort = null;
                    return false;
            }
        }

        public bool SendDataStream(byte[] data)
        {
            try
            {
                // Serial communication
                if (serialPort != null && tcpClient == null)
                {
                    return true;
                }
                // TCP communication
                else if (serialPort == null && tcpClient != null)
                {
                    if (!tcpClient.Connected)
                    {
                        return false;
                    }
                    else
                    {
                        NetworkStream dataStream = tcpClient.GetStream();
                        dataStream.Write(data, 0, data.Length);
                        Console.WriteLine(BitConverter.ToString(data));
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        
        #region Serial Communication

        public bool SerialConnect(string comPort, int baudrate, out SerialPort serialPort)
        {
            serialPort = null;

            string[] ports = SerialPort.GetPortNames();

            if (ports.Length != 0)
            {
                for (int i = 0; i < ports.Length; i++)
                {
                    if (ports[i].Equals(comPort))
                    {
                        serialPort = new SerialPort(comPort, baudrate, Parity.None, 8, StopBits.One);

                        break;
                    }
                }

                //check if _serialPort not constructed means no connection
                if (serialPort != null)
                {
                    //serialPort.DataReceived += SerialPortDataReceived;
                    serialPort.Open();
                    return true;
                }
                else
                {
                    MyDebug.WriteLine("Serial Port not found.");
                }
            }
            else
            {
                MyDebug.WriteLine("No Serial Port Avaliable");
            }

            return false;
        }

        #endregion Serial Communication

        #region TCP Connection

        public bool TcpConnect(string hostName, int port, int timeout)
        {
            IPAddress ip;

            if (IPAddress.TryParse(hostName, out ip))
            {

                if (TcpPing(hostName, timeout).Status == IPStatus.Success)
                {
                    tcpClient = new TcpClient(hostName, port);
                    return true;
                }
                else
                {
                    tcpClient = null;
                    return false;
                }
            }
            else
            {
                MyDebug.WriteLine("Invalid IP Address");
            }
            return false;
        }

        public PingReply TcpPing(string host, int timeout)
        {
            try
            {
                PingReply pingReply = new Ping().Send(host, timeout);
                return pingReply;
            }
            catch
            {
                Debug.WriteLine("TCP Connection Failed -----------------> Ping Unsuccessful!");
            }
            return null;
        }

        #endregion TCP Connection

        protected void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}