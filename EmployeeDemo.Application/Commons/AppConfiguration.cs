namespace EmployeeDemo.Application.Commons
{
    public class AppConfiguration
    {
        // values are bound from configuration at startup
        public string DatabaseConnection { get; set; } = null!;
        public string JWTSecretKey { get; set; } = null!;
    }
}
