namespace AuthenticationService.Repositories
{
    public class ServiceRepository:DbContextProvider
    {
        AuthenticationRepository authentication;
        public AuthenticationRepository Authentication
        {
            get
            {
                if (authentication == null)
                {
                    authentication = new AuthenticationRepository(Context);
                }
                return authentication;
            }
        }
    }
}
