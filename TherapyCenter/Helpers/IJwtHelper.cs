using TherapyCenter.Models;

namespace TherapyCenter.Helpers
{
    public interface IJwtHelper
    {
        string GenerateToken(User user);
    }
}