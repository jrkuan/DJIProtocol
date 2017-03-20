using DJIProtocolTest.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJIProtocolTest.Models.DJIProtocol
{
    public abstract class DJIMission { }
    
    public class DJIWaypointMission : DJIMission
    {
        #region Constants

        public const int DJIWaypointMissionMaximumWaypointCount = 99;
        public const int DJIWaypointMissionMinimumWaypointCount = 1;

        public enum DJIWaypointMissionHeadingMode
        {
            Auto,
            UsingInitialDirection,
            ControlByRemoteController,
            UsingWaypointHeading,
            TowardsPointOfInterest
        }

        public enum DJIWaypointMissionFinishedAction
        {
            NoAction,
            GoHome,
            AutoLand,
            GoFirstWaypoint,
            ContinueUntilEnd
        }

        public enum DJIWaypointMissionFlightPathMode
        {
            Normal,
            Curved
        }

        public enum DJIGoToWaypointMode
        {
            PointToPoint,
            Safely
        }

        #endregion Constants

        #region Fields

        /// <summary>
        /// The base automatic speed of the aircraft as it moves between waypoints with range [-15, 15] m/s
        /// </summary>
        public float autoFlightSpeed;

        /// <summary>
        /// While the aircraft is travelling between waypoints, you can offset its speed by using the throttle joystick on the remote controller.
        /// </summary>
        public float maxFlightSpeed;

        /// <summary>
        /// How many times the task will be repeated, meaning if repeatNum is 1, the aircraft will return to the first waypoint after finishing
        /// the task, and repeat the task once more.
        /// </summary>
        public int repeatNum;

        /// <summary>
        /// ArrayList of waypoints in the DJIWaypointMission.
        /// </summary>
        public List<DJIWaypoint> waypointsList = new List<DJIWaypoint>();

        /// <summary>
        /// Determines whether to exit the mission when the RC signal is lost.
        /// </summary>
        public bool needExitMissionOnRCSignalLost;

        /// <summary>
        /// Determines whether the aircraft can rotate the gimbal pitch when executing a waypoint mission.
        /// </summary>
        public bool needRotateGimbalPitch;

        /// <summary>
        /// Heading of the aircraft as it moves between waypoints.
        /// </summary>
        public DJIWaypointMissionHeadingMode headingMode;

        /// <summary>
        /// What type of path is generated for the aircraft to follow.
        /// </summary>
        public DJIWaypointMissionFlightPathMode flightPathMode;

        /// <summary>
        /// The action the aircraft will take after completing the last waypoint.
        /// </summary>
        public DJIWaypointMissionFinishedAction finishedAction;

        /// <summary>
        /// Determines how the aircraft goes to the first waypoint from the current location
        /// </summary>
        public DJIGoToWaypointMode goFirstWaypointMode;

        /// <summary>
        /// Used when "headingMode" is "DJIWaypointMissionHeadingMode.TowardPointOfInterest"
        /// </summary>
        public double pointOfInterestLatitude;

        /// <summary>
        /// Used when "headingMode" is "DJIWaypointMissionHeadingMode.TowardPointOfInterest"
        /// </summary>
        public double pointOfInterestLongitude;

        public double pointOfInterestAltitude;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Add a waypoint to the waypoint mission.
        /// </summary>
        public void AddWaypoint(DJIWaypoint waypoint)
        {
            waypointsList.Add(waypoint);
        }

        /// <summary>
        /// Adds a list of waypoints to the waypoint mission
        /// </summary>
        public void AddWaypoints(List<DJIWaypoint> waypoints)
        {
            waypointsList.AddRange(waypoints);
        }

        /// <summary>
        /// Gets a waypoint at an index in the mission waypoint array.
        /// </summary>
        public DJIWaypoint GetWaypointAtIndex(int index)
        {
            return waypointsList[index];
        }

        /// <summary>
        /// Number of waypoints in the waypoint mission.
        /// </summary>
        public int GetWaypointCount()
        {
            if (waypointsList.Count != 0)
            {
                return waypointsList.Count;
            }
            else
            {
                waypointsList = new List<DJIWaypoint>();
                return 0;
            }
            
        }

        /// <summary>
        /// Removes all waypoints from the waypoint mission.
        /// </summary>
        public void RemoveAllWaypoints()
        {
            waypointsList.Clear();
        }

        /// <summary>
        /// Removes the waypoint at an index.
        /// </summary>
        public void RemoveWaypointAtIndex(int index)
        {
            waypointsList.RemoveAt(index);
        }

        #endregion Methods

    }

    public class DJIWaypoint
    {
        /// <summary>
        /// This class represents a waypoint action for DJIWaypoint.
        /// </summary>
        public class DJIWaypointAction
        {
            public int mActionParam;
            public DJIWaypointActionType mActionType;

            public DJIWaypointAction(DJIWaypointActionType mActionType, int mActionParam)
            {
                this.mActionType = mActionType;
                this.mActionParam = mActionParam;
            }
        }

        #region Constants

        /// <summary>
        /// Waypoint action types.
        /// </summary>
        public enum DJIWaypointActionType
        {
            STAY,
            STARTTAKEPHOTO,
            STARTRECORD,
            STOPRECORD,
            ROTATEAIRCRAFT,
            GIMBALPITCH
        }

        /// <summary>
        /// How the aircraft will turn at a waypoint to transition between headings.
        /// </summary>
        public enum DJIWaypointTurnMode
        {
            Clockwise,
            CounterClockwise
        }

        /// <summary>
        /// Maximum number of actions a single waypoint can have. Currently, the maximum number supported is 15.
        /// </summary>
        public const int DJIMaxActionCount = 15;

        /// <summary>
        /// Maximum number of times a single waypoint action can be repeated. Currently, the maximum number supported is 15.
        /// </summary>
        public const int DJIMaxActionRepeatTimes = 15;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Dictates how many times the set of waypoint actions are repeated.
        /// </summary>
        public int actionRepeatTimes;

        /// <summary>
        /// The maximum time set to excute all the waypoint actions for a waypoint.
        /// </summary>
        public short actionTimeoutInSeconds;

        /// <summary>
        /// Altitude of the aircraft in meters when it reaches waypoint.
        /// </summary>
        public float altitude;

        /// <summary>
        /// Corner radius of the waypoint.
        /// </summary>
        public float cornerRadiusInMeters;

        /// <summary>
        /// Gimbal pitch angle when reached this waypoint.
        /// </summary>
        public float gimbalPitch;

        /// <summary>
        /// A flag to indicate whether there are any actions to be carried out at the waypoint.
        /// </summary>
        public bool hasAction;

        /// <summary>
        /// Heading the aircraft will rotate to by the time it reaches the waypoint. 
        /// The aircraft heading will gradually change between two waypoints with different 
        /// headings if the waypoint mission's headingMode is set to DJIWaypointMissionHeadingUsingWaypointHeading. 
        /// Heading has a range of [-180, 180] degrees, where 0 represents True North.
        /// </summary>
        public short heading;

        /// <summary>
        /// Waypoint coordinate latitude in degrees.
        /// </summary>
        public double latitude;

        /// <summary>
        /// Waypoint coordinate longitude in degrees.
        /// </summary>
        public double longitude;

        /// <summary>
        /// Determines whether the aircraft will turn clockwise or anti-clockwise when changing its heading.
        /// </summary>
        public DJIWaypointTurnMode turnMode;

        /// <summary>
        /// ArrayList of all waypoint actions for the respective waypoint.
        /// </summary>
        public List<DJIWaypointAction> waypointActions;

        #endregion Fields

        /// <summary>
        /// Construct instance with specific waypoint.
        /// </summary>
        public DJIWaypoint(double latitude, double longitude, float altitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.altitude = altitude;
            this.waypointActions = new List<DJIWaypointAction>();
        }

        #region Methods

        /// <summary>
        /// Adds a waypoint action to a waypoint.
        /// </summary>
        public void AddAction(DJIWaypointAction action)
        {
            waypointActions.Add(action);
        }

        /// <summary>
        /// Switches the action at the specified index.
        /// </summary>
        public void AdjustActionAtIndex()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the action at the specified index.
        /// </summary>
        public DJIWaypointAction GetActionAtIndex(int index)
        {
            return waypointActions[index];
        }

        /// <summary>
        /// Insert a waypoint action at index.
        /// </summary>
        public void InsertAction(DJIWaypointAction action, int index)
        {
            waypointActions.Insert(index, action);
        }

        /// <summary>
        /// Removes the waypoint action from the waypoint.
        /// </summary>
        public void RemoveAction(DJIWaypointAction action)
        {
            waypointActions.Remove(action);
        }

        /// <summary>
        /// Removes a waypoint action from the waypoint by index.
        /// </summary>
        public void RemoveActionAtIndex(int index)
        {
            waypointActions.RemoveAt(index);
        }

        /// <summary>
        /// Remove all the actions.
        /// </summary>
        public void RemoveAllActions()
        {
            waypointActions.Clear();
        }

        #endregion Methods
    }

    public class DJIFollowMeMission : DJIMission
    {
        #region Constants

        /// <summary>
        /// Aircraft's heading during a follow me mission.
        /// </summary>
        public enum DJIFollowMeHeading
        {
            TowardFollowPosition,
            ControlledByRemoteController
        }

        #endregion Constants

        #region Fields

        /// <summary>
        /// User's initial altitude (above sea level).
        /// </summary>
        public static float followMeAltitude;

        /// <summary>
        /// Latitude of the user's initial coordinate.
        /// </summary>
        public static double followMeLatitude;

        /// <summary>
        /// Longitude of the user's initial coordinate.
        /// </summary>
        public static double followMeLongitude;

        /// <summary>
        /// The aircraft's heading during the mission.
        /// </summary>
        public DJIFollowMeHeading heading;

        #endregion Fields

        public DJIFollowMeMission(double userLatitude, double userLongitude, float userAltitude)
        {
            followMeLatitude = userLatitude;
            followMeLongitude = userLongitude;
            followMeAltitude = userAltitude;
        }

        public DJIFollowMeMission(double userLatitude, double userLongitude)
        {
            followMeLatitude = userLatitude;
            followMeLongitude = userLongitude;
        }

        public static void UpdateFollowMeCoordinate(double targetLatitude, double targetLongitude, float targetAltitude)
        {    
            followMeLatitude = targetLatitude;
            followMeLongitude = targetLongitude;
            followMeAltitude = targetAltitude;
        }
    }
}