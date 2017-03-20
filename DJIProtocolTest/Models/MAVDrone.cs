using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJIProtocolTest.Models
{
    class MAVDrone : Drone
    {
        public override string Param1 { get; set; }
        public override int Param2 { get; set; }

        public override bool AutoReconnect { get; set; } = true;

        public override bool IsConnected { get; set; } = false;

        public override ConnectionType ConnType { get; set; }

        public override void Connect()
        {
            StartConnection(this);
        }

        public override DroneStatus Status
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Timeout
        {
            get
            ;

            set
            ;
        }

        public override long RoundTripTime
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool StopTcpClient
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override ConnectionType GetConnectionType()
        {
            throw new NotImplementedException();
        }

        public override bool Arm()
        {
            throw new NotImplementedException();
        }

        public override bool Disarm()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Activate and Set Frequency
        /// </summary>
        public override bool InitiatePreSettings()
        {
            //activate

            //set freq
            throw new NotImplementedException();
        }

        public override bool Launch()
        {
            throw new NotImplementedException();
        }

        public override bool Land()
        {
            throw new NotImplementedException();
        }

        public override bool ReturnToHome()
        {
            throw new NotImplementedException();
        }

        public override bool UploadWaypointMission()
        {
            throw new NotImplementedException();
        }

        public override bool StartWaypointMission()
        {
            throw new NotImplementedException();
        }

        public override bool DownloadWaypointMission()
        {
            throw new NotImplementedException();
        }

        public override bool NudgeInDirection(float heading, double distance)
        {
            throw new NotImplementedException();
        }

        public override bool StopWaypointMission()
        {
            throw new NotImplementedException();
        }

        public override bool StartFollowMeMission()
        {
            throw new NotImplementedException();
        }

        public override bool StopFollowMeMission()
        {
            throw new NotImplementedException();
        }

        public override bool HighLaunch()
        {
            throw new NotImplementedException();
        }

        public override bool FlyToHere()
        {
            throw new NotImplementedException();
        }

        public override void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void TcpClientDataReceived()
        {
            throw new NotImplementedException();
        }
    }
}
