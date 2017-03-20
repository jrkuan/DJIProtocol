using DJIProtocolTest.Models;
using DJIProtocolTest.Models.DJIProtocol;
using DJIProtocolTest.Models.Helpers;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DJIProtocolTest.ViewModels
{
    public class Core
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        public ObservableCollection<Drone> AllDrones = new ObservableCollection<Drone>();

        public Core()
        {
            AllocConsole();
            AllDrones.Add(new DJIDrone() { ConnType = Drone.ConnectionType.TCP, Param1 = "10.0.1.21", Param2 = 5000});
            AllDrones[0].Connect();
            AllDrones.Add(new MAVDrone());
        }

        #region DelegateCommands

        private DelegateCommand armCommand;
        private DelegateCommand disarmCommand;
        private DelegateCommand takeoffCommand;
        private DelegateCommand landCommand;
        private DelegateCommand uploadMissionCommand;
        private DelegateCommand downloadMisionCommand;
        private DelegateCommand returnHomeCommand;
        private DelegateCommand highLaunchCommand;
        private DelegateCommand nudgeCommand;
        private DelegateCommand startWaypointMissionCommand;

        public DelegateCommand ArmCommand
        {
            get
            {
                return armCommand = new DelegateCommand(Arm);
            }
        }
        public DelegateCommand DisarmCommand
        {
            get
            {
                return disarmCommand = new DelegateCommand(Disarm);
            }
        }
        public DelegateCommand TakeoffCommand
        {
            get
            {
                return takeoffCommand = new DelegateCommand(Takeoff);
            }
        }
        public DelegateCommand LandCommand
        {
            get
            {
                return landCommand = new DelegateCommand(Land);
            }
        }
        public DelegateCommand UploadMissionCommand
        {
            get
            {
                return uploadMissionCommand = new DelegateCommand(UploadMission);
            }
        }
        public DelegateCommand DownloadMissionCommand
        {
            get
            {
                return downloadMisionCommand = new DelegateCommand(DownloadMission);
            }
        }
        public DelegateCommand ReturnHomeCommand
        {
            get
            {
                return returnHomeCommand = new DelegateCommand(ReturnHome);
            }
        }
        public DelegateCommand HighLaunchCommand
        {
            get
            {
                return highLaunchCommand = new DelegateCommand(HighLaunch);
            }
        }
        public DelegateCommand NudgeCommand
        {
            get
            {
                return nudgeCommand = new DelegateCommand(Nudge);
            }
        }            
        public DelegateCommand StartWaypointMissionCommand
        {
            get
            {
                return startWaypointMissionCommand = new DelegateCommand(StartWaypointMission);
            }
        }

        #endregion DelegateCommands

        #region Commands

        private void Arm()
        {
            Debug.WriteLine("Arm Button Clicked");
            AllDrones[0].Arm();
        }

        private void Disarm()
        {
            AllDrones[0].Disarm();
        }

        private void Takeoff()
        {
            AllDrones[0].Launch();
        }

        private void Land()
        {
            AllDrones[0].Land();
        }

        private void UploadMission()
        {
            AllDrones[0].UploadWaypointMission();
        }

        private void StartWaypointMission()
        {
            AllDrones[0].StartWaypointMission();
        }

        private void DownloadMission()
        {
            AllDrones[0].DownloadWaypointMission();
        }

        private void ReturnHome()
        {
            AllDrones[0].ReturnToHome();
        }

        private void HighLaunch()
        {
            AllDrones[0].HighLaunch();
        }

        private void Nudge()
        {

        }

        #endregion Commands

    }
}