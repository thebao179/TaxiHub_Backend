using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
    public static class Catalouge
    {
        public static class Trip
        {
            public static string Done = "Finished";
            public static string CanceledByDriver = "CanceledByDriver";
            public static string CanceledByCus = "CanceledByCus";
            public static string PickingUpCus = "PickingUpCus";
            public static string OnTheWay = "OnTheWay";
        }

        public static class Request
        {
            public static string FindingDriver = "FindingDriver";
            public static string MovedToTrip = "MovedToTrip";
            public static string Canceled = "Canceled";
            public static string TimeOut = "TimeOut";
        }

        public static class Role
        {
            public static string Driver = "Driver";
            public static string Staff = "Staff";
            public static string Passenger = "Passenger";
        }

        public static class VehicleType
        {
            public static string Motobike = "Motobike";
            public static string Car4S = "Car4S";
            public static string Car7S = "Car7S";
        }

        public static class Gender
        {
            public static bool Male = true;
            public static bool Female = false;
        }
    }
}
