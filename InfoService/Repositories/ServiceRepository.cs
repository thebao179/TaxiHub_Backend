namespace InfoService.Repositories
{
    public class ServiceRepository : DbContextProvider
    {
        DriverRespository driver;
        public DriverRespository Driver
        {
            get
            {
                if (driver == null)
                {
                    driver = new DriverRespository(Context);
                }
                return driver;
            }
        }

        StaffRepository staff;
        public StaffRepository Staff
        {
            get
            {
                if (staff == null)
                {
                    staff = new StaffRepository(Context);
                }
                return staff;
            }
        }

        PassengerRepository passenger;
        public PassengerRepository Passenger
        {
            get
            {
                if (passenger == null)
                {
                    passenger = new PassengerRepository(Context);
                }
                return passenger;
            }
        }

        VehicleRepository vehicle;
        public VehicleRepository Vehicle
        {
            get
            {
                if (vehicle == null)
                {
                    vehicle = new VehicleRepository(Context);
                }
                return vehicle;
            }
        }
    }
}
