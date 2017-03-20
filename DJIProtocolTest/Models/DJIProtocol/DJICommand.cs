using DJIProtocolTest.Models.DJIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJIProtocolTest.Models.DJIProtocol
{
    public class DJICommand
    {

        #region Constants

        private const byte StartOfFrame = 0xAA;
        private const byte ReservedByte = 0x00;
        private static ushort seqNo = 0x00;
        private static byte cmdSeqNo = 0xCE;

        private const double DEG2RAD = 0.01745329252;
        private const double RAD2DEG = 1 / DEG2RAD;

        #region Enums

        public enum FlightMode
        {
            ReturnToHome = 0x01,
            AutoTakeOff = 0x04,
            AutoLanding = 0x06
        }

        private enum CMD_SET
        {
            INIT = (byte)0x00,
            CTRL = (byte)0x01,
            PUSH_DATA = (byte)0x02,
            GND_STN = (byte)0x03,
        }
        private enum CMD_ID_INIT
        {
            GET_PROTOCOL_VER = (byte)0x00,
            ACTIVATION = (byte)0x01,
            SET_FLIGHT_DATA_FREQ = (byte)0x10
        }
        private enum CMD_ID_CTRL
        {
            OBTAIN_REL_CTRL = (byte)0x00,
            SWITCH_FLIGHT_MODE = (byte)0x01,
            REQUEST_FLIGHT_MODE = (byte)0x02,
            MOVEMENT_CTRL = (byte)0x03,
            ARM_DISARM = (byte)0x05,
            GIMBAL_CONTROL_RATE = (byte)0x1A,
            GIMBAL_CONTROL_POS = (byte)0x1B,
            TAKE_PHOTO = (byte)0x20,
            START_RECORDING = (byte)0x21,
            STOP_RECORDING = (byte)0x22
        }
        private enum CMD_ID_PUSH_DATA
        {
            FLIGHT_DATA = (byte)0x00,
            LOSS_FLIGHT_CTRL = (byte)0x01,
            GND_STN_STATE = (byte)0x03,
            WP_EVENT = (byte)0x04
        }
        private enum CMD_ID_GND_STN
        {
            UPL_WP_MISSION_SET = (byte)0x10,
            UPL_WP_DATA = (byte)0x11,
            START_STOP_WP_MISSION = (byte)0x12,
            PAUSE_RESUME_WP_MISSION = (byte)0x13,
            SET_WP_MISSION_IDLE = (byte)0x16,
            GET_WP_MISSION_IDLE = (byte)0x17,
            UPLOAD_START_HOTPOINT = (byte)0x20,
            STOP_HOTPOINT = (byte)0x21,
            PAUSE_RESUME_HOTPOINT = (byte)0x22,
            GET_WP_INIT_STATUS = (byte)0x14,
            GET_SINGLE_WP_STATUS = (byte)0x15,
            UPLOAD_START_FOLLOWME = (byte)0x30,
            STOP_FOLLOWME = (byte)0x31,
            PAUSE_RESUME_FOLLOWME = (byte)0x32,
            UPDATE_TARGET_POS_FOLLOWME = (byte)0x33,
        }

        private enum HEADER_SESSION
        {
            GET_PROTOCOL_VER,
            ACTIVATION,
            SET_FLIGHT_DATA_FREQ,
            OBTAIN_REL_CTRL,
            SWITCH_FLIGHT_MODE,
            MOVEMENT_CTRL,
            ARM_DISARM,
            FLIGHT_DATA,
            LOSS_FLIGHT_CTRL,
            GND_STN_STATE,
            WP_EVENT,
            UPL_WP_MISSION_SET = 2,
            UPL_WP_DATA = 2,
            START_STOP_WP_MISSION = 2,
            PAUSE_RESUME_WP_MISSION = 2,
            SET_WP_MISSION_IDLE = 2,
            GET_WP_MISSION_IDLE = 2
        }

        #endregion

        #region CRC Tables

        private uint[] crc_tab16 = {0x0000, 0xc0c1, 0xc181, 0x0140, 0xc301, 0x03c0, 0x0280, 0xc241, 0xc601, 0x06c0, 0x0780,
                                    0xc741, 0x0500, 0xc5c1, 0xc481, 0x0440, 0xcc01, 0x0cc0, 0x0d80, 0xcd41, 0x0f00, 0xcfc1,
                                    0xce81, 0x0e40, 0x0a00, 0xcac1, 0xcb81, 0x0b40, 0xc901, 0x09c0, 0x0880, 0xc841, 0xd801,
                                    0x18c0, 0x1980, 0xd941, 0x1b00, 0xdbc1, 0xda81, 0x1a40, 0x1e00, 0xdec1, 0xdf81, 0x1f40,
                                    0xdd01, 0x1dc0, 0x1c80, 0xdc41, 0x1400, 0xd4c1, 0xd581, 0x1540, 0xd701, 0x17c0, 0x1680,
                                    0xd641, 0xd201, 0x12c0, 0x1380, 0xd341, 0x1100, 0xd1c1, 0xd081, 0x1040, 0xf001, 0x30c0,
                                    0x3180, 0xf141, 0x3300, 0xf3c1, 0xf281, 0x3240, 0x3600, 0xf6c1, 0xf781, 0x3740, 0xf501,
                                    0x35c0, 0x3480, 0xf441, 0x3c00, 0xfcc1, 0xfd81, 0x3d40, 0xff01, 0x3fc0, 0x3e80, 0xfe41,
                                    0xfa01, 0x3ac0, 0x3b80, 0xfb41, 0x3900, 0xf9c1, 0xf881, 0x3840, 0x2800, 0xe8c1, 0xe981,
                                    0x2940, 0xeb01, 0x2bc0, 0x2a80, 0xea41, 0xee01, 0x2ec0, 0x2f80, 0xef41, 0x2d00, 0xedc1,
                                    0xec81, 0x2c40, 0xe401, 0x24c0, 0x2580, 0xe541, 0x2700, 0xe7c1, 0xe681, 0x2640, 0x2200,
                                    0xe2c1, 0xe381, 0x2340, 0xe101, 0x21c0, 0x2080, 0xe041, 0xa001, 0x60c0, 0x6180, 0xa141,
                                    0x6300, 0xa3c1, 0xa281, 0x6240, 0x6600, 0xa6c1, 0xa781, 0x6740, 0xa501, 0x65c0, 0x6480,
                                    0xa441, 0x6c00, 0xacc1, 0xad81, 0x6d40, 0xaf01, 0x6fc0, 0x6e80, 0xae41, 0xaa01, 0x6ac0,
                                    0x6b80, 0xab41, 0x6900, 0xa9c1, 0xa881, 0x6840, 0x7800, 0xb8c1, 0xb981, 0x7940, 0xbb01,
                                    0x7bc0, 0x7a80, 0xba41, 0xbe01, 0x7ec0, 0x7f80, 0xbf41, 0x7d00, 0xbdc1, 0xbc81, 0x7c40,
                                    0xb401, 0x74c0, 0x7580, 0xb541, 0x7700, 0xb7c1, 0xb681, 0x7640, 0x7200, 0xb2c1, 0xb381,
                                    0x7340, 0xb101, 0x71c0, 0x7080, 0xb041, 0x5000, 0x90c1, 0x9181, 0x5140, 0x9301, 0x53c0,
                                    0x5280, 0x9241, 0x9601, 0x56c0, 0x5780, 0x9741, 0x5500, 0x95c1, 0x9481, 0x5440, 0x9c01,
                                    0x5cc0, 0x5d80, 0x9d41, 0x5f00, 0x9fc1, 0x9e81, 0x5e40, 0x5a00, 0x9ac1, 0x9b81, 0x5b40,
                                    0x9901, 0x59c0, 0x5880, 0x9841, 0x8801, 0x48c0, 0x4980, 0x8941, 0x4b00, 0x8bc1, 0x8a81,
                                    0x4a40, 0x4e00, 0x8ec1, 0x8f81, 0x4f40, 0x8d01, 0x4dc0, 0x4c80, 0x8c41, 0x4400, 0x84c1,
                                    0x8581, 0x4540, 0x8701, 0x47c0, 0x4680, 0x8641, 0x8201, 0x42c0, 0x4380, 0x8341, 0x4100,
                                    0x81c1, 0x8081, 0x4040};

        private uint[] crc_tab32 = { 0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419, 0x706af48f, 0xe963a535,
                                     0x9e6495a3, 0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd,
                                     0xe7b82d07, 0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de, 0x1adad47d,
                                     0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec,
                                     0x14015c4f, 0x63066cd9, 0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4,
                                     0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b, 0x35b5a8fa, 0x42b2986c,
                                     0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac,
                                     0x51de003a, 0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f,
                                     0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924, 0x2f6f7c87, 0x58684c11, 0xc1611dab,
                                     0xb6662d3d, 0x76dc4190, 0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
                                     0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb,
                                     0x086d3d2d, 0x91646c97, 0xe6635c01, 0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e,
                                     0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950, 0x8bbeb8ea,
                                     0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65, 0x4db26158, 0x3ab551ce,
                                     0xa3bc0074, 0xd4bb30e2, 0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a,
                                     0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9,
                                     0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409,
                                     0xce61e49f, 0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81,
                                     0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a, 0xead54739,
                                     0x9dd277af, 0x04db2615, 0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
                                     0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344, 0x8708a3d2, 0x1e01f268,
                                     0x6906c2fe, 0xf762575d, 0x806567cb, 0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0,
                                     0x10da7a5a, 0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5, 0xd6d6a3e8,
                                     0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
                                     0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef,
                                     0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236, 0xcc0c7795, 0xbb0b4703,
                                     0x220216b9, 0x5505262f, 0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7,
                                     0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a,
                                     0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713, 0x95bf4a82, 0xe2b87a14, 0x7bb12bae,
                                     0x0cb61b38, 0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
                                     0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777, 0x88085ae6,
                                     0xff0f6a70, 0x66063bca, 0x11010b5c, 0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45,
                                     0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7, 0x4969474d,
                                     0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5,
                                     0x47b2cf7f, 0x30b5ffe9, 0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605,
                                     0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94,
                                     0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d};

        #endregion CRC Tables

        #endregion Constants

        #region Fields

        private byte[] _body;
        private readonly byte HEADER_VERSION = 0x00;

        #endregion Fields

        #region Methods

        #region Commands

        #region Initialization CMD Set

        public byte[] GetProtocolVersion()
        {
            _body = new byte[1] { 0 };

            return DoFrame((byte)CMD_SET.INIT, (byte)CMD_ID_INIT.GET_PROTOCOL_VER, _body);
        }

        /// <summary>
        /// Activation. Hard coded to A3 ver 3.02.15.37
        /// </summary>
        public byte[] Activation()
        {
            _body = new byte[44];
            Array.Clear(_body, 0, 44);

            _body[4] = (byte)0x02;

            byte[] deviceVersion = { 0x25, 0x0f, 0x02, 0x03 };
            deviceVersion.CopyTo(_body, 8);

            for (int i = 0; i < 32; i++)
            {
                _body[12 + i] = (byte)0x30;
            }

            return DoFrame((byte)CMD_SET.INIT, (byte)CMD_ID_INIT.ACTIVATION, _body);
        }

        /// <summary>
        /// Activation for given protocol version.
        /// </summary>
        public byte[] Activation(byte[] version)
        {
            _body = new byte[44];
            Array.Clear(_body, 0, 44);

            _body[4] = (byte)0x02;

            // Assert
            if (version.Length != 4)
            {
                return Activation();
            }

            version.CopyTo(_body, 8);

            for (int i = 0; i < 32; i++)
            {
                _body[12 + i] = (byte)0x30;
            }

            return DoFrame((byte)CMD_SET.INIT, (byte)CMD_ID_INIT.ACTIVATION, _body);
        }

        /// <summary>
        /// Set flight data frequency.
        /// </summary>
        public byte[] SetFlightDataFrequency(byte frequency)
        {
            _body = new byte[16];

            for (int i = 0; i < 14; i++)
            {
                _body[i] = frequency;
            }

            _body[14] = ReservedByte;
            _body[15] = ReservedByte;

            return DoFrame((byte)CMD_SET.INIT, (byte)CMD_ID_INIT.SET_FLIGHT_DATA_FREQ, _body);
        }

        /// <summary>
        /// Set default flight data frequency
        /// </summary>
        public byte[] SetFlightDataFrequency()
        {
            _body = new byte[16];

            _body[0] = (byte)0x04;
            _body[1] = (byte)0x04;
            _body[2] = (byte)0x04;
            _body[3] = (byte)0x04;
            _body[4] = (byte)0x04;
            _body[5] = (byte)0x04;
            _body[6] = (byte)0x03;
            _body[7] = (byte)0x03;
            _body[8] = (byte)0x00;
            _body[9] = (byte)0x03;
            _body[10] = (byte)0x03;
            _body[11] = (byte)0x02;
            _body[12] = (byte)0x01;
            _body[13] = (byte)0x00;
            _body[14] = ReservedByte;
            _body[15] = ReservedByte;

            return DoFrame((byte)CMD_SET.INIT, (byte)CMD_ID_INIT.SET_FLIGHT_DATA_FREQ, _body);
        }

        /// <summary>
        /// Switch flight data frequency between mimimum and disabled.
        /// </summary>
        public byte[] SetFlightDataFrequency(bool enable)
        {
            _body = new byte[16];

            if (enable)
            {
                _body[0] = (byte)0x02;
                _body[1] = (byte)0x02;
                _body[2] = (byte)0x00;
                _body[3] = (byte)0x00;
                _body[4] = (byte)0x00;
                _body[5] = (byte)0x02;
                _body[6] = (byte)0x02;
                _body[7] = (byte)0x02;
                _body[8] = (byte)0x00;
                _body[9] = (byte)0x00;
                _body[10] = (byte)0x00;
                _body[11] = (byte)0x01;
                _body[12] = (byte)0x00;
                _body[13] = (byte)0x00;
                _body[14] = ReservedByte;
                _body[15] = ReservedByte;
            }
            else
            {
                Array.Clear(_body, 0, _body.Length);
            }
            return DoFrame((byte)CMD_SET.INIT, (byte)CMD_ID_INIT.SET_FLIGHT_DATA_FREQ, _body);
        }

        #endregion Initialization CMD Set

        #region Control CMD Set

        public byte[] ObtainControl()
        {
            _body = new byte[1] { 1 };

            return DoFrame((byte)CMD_SET.CTRL, (byte)CMD_ID_CTRL.OBTAIN_REL_CTRL, _body);
        }

        public byte[] ObtainReleaseControl(bool obtain)
        {
            _body = new byte[1];

            _body[0] = obtain ? (byte)0x01 : (byte)0x00;
            return DoFrame((byte)CMD_SET.CTRL, (byte)CMD_ID_CTRL.OBTAIN_REL_CTRL, _body);
        }
        
        /// <summary>
        /// </summary>
        public byte[] SwitchFlightMode(FlightMode flightMode)
        {
            _body = new byte[2];

            _body[0] = cmdSeqNo++;

            switch (flightMode)
            {
                case FlightMode.ReturnToHome:
                    _body[1] = 0x01;
                    break;

                case FlightMode.AutoTakeOff:
                    _body[1] = 0x04;
                    break;

                case FlightMode.AutoLanding:
                    _body[1] = 0x06;
                    break;

                default:
                    _body[1] = 0x00;
                    break;
            }

            return DoFrame((byte)CMD_SET.CTRL, (byte)CMD_ID_CTRL.SWITCH_FLIGHT_MODE, _body);
        }

        public byte[] RequestSwitchResult()
        {
            _body = new byte[1] { cmdSeqNo++ };         

            return DoFrame((byte)CMD_SET.CTRL, (byte)CMD_ID_CTRL.REQUEST_FLIGHT_MODE, _body);
        }

        public byte[] MovementControl(byte controlMode, float roll, float pitch, float throttle, float yaw)
        {
            _body = new byte[17];
            _body[0] = controlMode;
            BitConverter.GetBytes(roll).CopyTo(_body, 1);
            BitConverter.GetBytes(pitch).CopyTo(_body, 5);
            BitConverter.GetBytes(throttle).CopyTo(_body, 9);
            BitConverter.GetBytes(yaw).CopyTo(_body, 13);

            return DoFrame((byte)CMD_SET.CTRL, (byte)CMD_ID_CTRL.MOVEMENT_CTRL, _body);
        }

        public byte[] Arm()
        {
            _body = new byte[1] { (byte)0x01 };
            return DoFrame((byte)CMD_SET.CTRL, (byte)CMD_ID_CTRL.ARM_DISARM, _body);
        }

        public byte[] Disarm()
        {
            _body = new byte[1] { (byte)0x00 };
            return DoFrame((byte)CMD_SET.CTRL, (byte)CMD_ID_CTRL.ARM_DISARM, _body);
        }

        public byte[] ArmDisarm(bool arm)
        {
            _body = new byte[1];
            _body[0] = arm ? (byte)0x01 : (byte)0x00;
            return DoFrame((byte)CMD_SET.CTRL, (byte)CMD_ID_CTRL.ARM_DISARM, _body);
        }

        public byte[] GimbalControlRate(double rate)
        {
            _body = new byte[7];
            return _body;
        }

        #endregion Control CMD Set

        #region Ground Station CMD Set

        #region Waypoint Mission CMD Set

        public byte[] UploadWaypointMissionSettings(DJIWaypointMission waypointMission)
        {
            _body = new byte[51];
            Array.Clear(_body, 0, _body.Length);

            _body[0] = (byte)waypointMission.GetWaypointCount();

            // Upload default max flight speed if invalid
            if (waypointMission.maxFlightSpeed <= 0)
            {
                // Default: 15
                new byte[]{ 0x00, 0x00, 0x70, 0x41 }.CopyTo(_body, 1);
            }
            else
            {
                BitConverter.GetBytes(waypointMission.maxFlightSpeed).CopyTo(_body, 1);
            }

            // Upload default idle flight speed if invalid
            if (waypointMission.autoFlightSpeed <= 0)
            {
                // Default: 3
                new byte[]{ 0x00, 0x00, 0x40, 0x40 }.CopyTo(_body, 5);
            }
            else
            {
                BitConverter.GetBytes(waypointMission.autoFlightSpeed).CopyTo(_body, 5);
            }

            _body[9] = (byte)waypointMission.finishedAction;                             //_body[9]

            // Execute once
            _body[10] = 0x01;

            _body[11] = (byte)waypointMission.headingMode;                                         //_body[11]
            _body[12] = (byte)waypointMission.flightPathMode;                                       //_body[12]
            _body[13] = waypointMission.needExitMissionOnRCSignalLost ? (byte)0x00 : (byte)0x01;
            _body[14] = waypointMission.needRotateGimbalPitch ? (byte)0x01 : (byte)0x00;           //_body[14]
            BitConverter.GetBytes(waypointMission.pointOfInterestLatitude).CopyTo(_body, 15);      //_body[15->22]
            BitConverter.GetBytes(waypointMission.pointOfInterestLongitude).CopyTo(_body, 23);     //_body[23->30]
            BitConverter.GetBytes(waypointMission.pointOfInterestAltitude).CopyTo(_body, 31);      //_body[31->34]

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.UPL_WP_MISSION_SET, _body);
        }

        public byte[] UploadWaypointData(DJIWaypointMission waypointMission, int index)
        {
            _body = new byte[90];
            Array.Clear(_body, 0, 90);

            DJIWaypoint waypoint = waypointMission.GetWaypointAtIndex(index);

            _body[0] = Convert.ToByte(index);
            BitConverter.GetBytes(waypoint.latitude * DEG2RAD).CopyTo(_body, 1);      //_body[1->8]          
            BitConverter.GetBytes(waypoint.longitude * DEG2RAD).CopyTo(_body, 9);     //_body[9->16]
            BitConverter.GetBytes(waypoint.altitude).CopyTo(_body, 17);     //_body[17->20]
            BitConverter.GetBytes(waypoint.cornerRadiusInMeters).CopyTo(_body, 21);      //_body[21->24]           
            _body[26] = BitConverter.GetBytes(waypoint.heading).ElementAt(0);   //_body[25->26]
            _body[25] = BitConverter.GetBytes(waypoint.heading).ElementAt(1);
            _body[28] = BitConverter.GetBytes(waypoint.gimbalPitch).ElementAt(0);  //_body[27->28]
            _body[27] = BitConverter.GetBytes(waypoint.gimbalPitch).ElementAt(1);
            _body[29] = (byte)waypoint.turnMode;        //_body[29]
            
            _body[38] = Convert.ToByte(waypoint.hasAction);                 //_body[38]
            _body[40] = 0x05;// BitConverter.GetBytes(waypoint.actionTimeLimit).ElementAt(0);  //_body[39->40]
            _body[39] = BitConverter.GetBytes(waypoint.actionTimeoutInSeconds).ElementAt(1);
            _body[41] = Convert.ToByte(waypoint.waypointActions.Count);              //_body[41]
            _body[41] += Convert.ToByte((Convert.ToByte(waypoint.actionRepeatTimes) << 4));

            if (waypoint.hasAction)
            {
                byte[] actionParamBytes = new byte[2];
                for (int i = 0; i < waypoint.waypointActions.Count; i++)
                {
                    _body[42 + i] = (byte)waypoint.waypointActions[i].mActionType;       //_body[42->57]
                    actionParamBytes = BitConverter.GetBytes(waypoint.waypointActions[i].mActionParam);      //_body[58->89]
                    _body[58 + (2 * i)] = actionParamBytes[1];
                    _body[58 + (2 * i) + 1] = actionParamBytes[0];
                }
            }

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.UPL_WP_DATA, _body);
        }

        public byte[] StartStopWaypointMission(bool start)
        {
            _body = new byte[1];
            _body[0] = start ? (byte)0x00 : (byte)0x01;

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.START_STOP_WP_MISSION, _body);
        }

        public byte[] StartWaypointMission()
        {
            _body = new byte[1] { (byte)0x00 };

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.START_STOP_WP_MISSION, _body);
        }

        public byte[] StopWaypointMission()
        {
            _body = new byte[1] { (byte)0x01 };

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.START_STOP_WP_MISSION, _body);
        }

        public byte[] PauseResumeWaypointMission(bool pause)
        {
            _body = new byte[1];
            _body[0] = pause ? (byte)0x00 : (byte)0x01;

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.PAUSE_RESUME_WP_MISSION, _body);
        }

        public byte[] PauseWaypointMission()
        {
            _body = new byte[1] { (byte)0x00 };

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.PAUSE_RESUME_WP_MISSION, _body);
        }

        public byte[] ResumeWaypointMission()
        {
            _body = new byte[1] { (byte)0x01 };

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.PAUSE_RESUME_WP_MISSION, _body);
        }

        public byte[] GetWaypointInitStatus()
        {
            _body = new byte[1] { (byte)0x00 };

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.GET_WP_INIT_STATUS, _body);
        }

        public byte[] GetSingleWaypointStatus(byte index)
        {
            _body = new byte[1] { (byte)index };

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.GET_SINGLE_WP_STATUS, _body);
        }

        public byte[] SetIdleSpeed(float idleVelocity)
        {
            _body = new byte[4];
            
            _body = BitConverter.GetBytes(idleVelocity);
            Array.Reverse(_body);

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.SET_WP_MISSION_IDLE, _body);
        }

        public byte[] GetIdleSpeed()
        {
            _body = new byte[1] { 0x00 };

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.GET_WP_MISSION_IDLE, _body);
        }

        #endregion Waypoint Mission CMD Set

        #region Follow Me Mission CMD Set

        public byte[] UploadAndStartFollowMe(DJIFollowMeMission mission)
        {
            _body = new byte[23];
            _body[0] = 0x00;
            _body[1] = (byte)mission.heading;
            BitConverter.GetBytes(DJIFollowMeMission.followMeLatitude).CopyTo(_body, 2);
            BitConverter.GetBytes(DJIFollowMeMission.followMeLongitude).CopyTo(_body, 10);
            BitConverter.GetBytes(DJIFollowMeMission.followMeAltitude).CopyTo(_body, 18);
            _body[22] = 0x00;

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.UPLOAD_START_FOLLOWME, _body);
        }

        public byte[] StopFollowMe()
        {
            _body = new byte[1] { (byte)0x00 };
            
            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.STOP_FOLLOWME, _body);
        }

        public byte[] PauseResumeFollowMeMission(bool pause)
        {
            _body = new byte[1];
            _body[0] = (pause) ? (byte)0 : (byte)1;
            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.PAUSE_RESUME_FOLLOWME, _body);
        }

        public byte[] PauseFollowMeMission()
        {
            _body = new byte[1] { (byte)0x00 };
            
            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.PAUSE_RESUME_FOLLOWME, _body);
        }

        public byte[] ResumeFollowMeMission()
        {
            _body = new byte[1] { (byte)0x01 };

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.PAUSE_RESUME_FOLLOWME, _body);
        }

        public byte[] UpdateTargetPosition(DJIFollowMeMission mission)
        {
            _body = new byte[20];
            Array.Clear(_body, 0, _body.Length);

            BitConverter.GetBytes(DJIFollowMeMission.followMeLatitude).CopyTo(_body, 0);
            BitConverter.GetBytes(DJIFollowMeMission.followMeLongitude).CopyTo(_body, 8);
            BitConverter.GetBytes(DJIFollowMeMission.followMeAltitude).CopyTo(_body, 16);

            return DoFrame((byte)CMD_SET.GND_STN, (byte)CMD_ID_GND_STN.UPDATE_TARGET_POS_FOLLOWME, _body);
        }

        #endregion Follow Me Mission CMD Set

        #endregion Ground Station CMD Set

        #endregion Commands

        #region Helpers

        public byte[] DoFrame(object cmdSet, object cmdId, byte[] _body)
        {
            byte[] frame = new byte[12 + 2 + _body.Length + 4];      // header + cmdSet/cmdId + _body + crc32

            byte[] header = DoHeader(HEADER_VERSION);

            /*modify len in header*/
            header[1] = (byte)(frame.Length);

            /*modify session in header*/
            header[3] = 0x02;

            /*calc crc16 for header*/
            byte[] crc16 = BitConverter.GetBytes(Stream_CRC16(header));
            header[10] = crc16[1];
            header[11] = crc16[0];

            /*copy header, cmdSet & cmdId into frame*/
            header.CopyTo(frame, 0);
            frame[12] = (byte)cmdSet;
            frame[13] = (byte)cmdId;
            _body.CopyTo(frame, 14);

            /*calc crc32 for frame*/
            byte[] crc32 = BitConverter.GetBytes(Stream_CRC32_No_Swap(frame));
            int frameLen = frame.Length;
            crc32.CopyTo(frame, frameLen - 4);


            return frame;
        }

        public byte[] DoHeader(uint headerVer)
        {
            byte[] header = new byte[12];
            Array.Clear(header, 0, 12);

            header[0] = StartOfFrame;

            byte[] tempSeqNo = BitConverter.GetBytes(seqNo++);
            header[8] = Convert.ToByte(tempSeqNo[0]);
            header[9] = Convert.ToByte(tempSeqNo[1]);

            return header;
        }

        public uint Stream_CRC16(Byte[] header)
        {
            uint wCRC = 0x3AA3;

            for (int i = 0; i < header.Count() - 2; i++)
            {
                wCRC = CRC16Update(wCRC, header[i]);
            }

            uint index_0 = (wCRC & 0x000000FF) << 8;
            uint index_1 = (wCRC & 0x0000FF00) >> 8;

            return index_0 + index_1;
        }

        private uint CRC16Update(uint crc, byte ch)
        {
            uint tmp;
            uint msg;

            msg = 0x00ff & (uint)ch;
            tmp = crc ^ msg;

            crc = (crc >> 8) ^ crc_tab16[tmp & 0xff];
            return crc;
        }

        public uint Stream_CRC32_No_Swap(Byte[] header)
        {
            uint wCRC = 0x3AA3;

            for (int i = 0; i < header.Count() - 4; i++)
            {
                wCRC = CRC32Update(wCRC, header[i]);
            }

            return wCRC;
        }

        private uint CRC32Update(uint crc, byte ch)
        {
            uint tmp;
            uint msg;

            msg = 0x000000ff & (UInt32)ch;
            tmp = crc ^ msg;
            crc = (crc >> 8) ^ crc_tab32[tmp & 0xff];
            return crc;
        }

        #endregion Helpers

        #endregion Methods

    }
}
