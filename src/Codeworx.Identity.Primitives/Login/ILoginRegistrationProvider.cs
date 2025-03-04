using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codeworx.Identity.Login
{
    public interface ILoginRegistrationProvider
    {
        Task<IEnumerable<ILoginRegistration>> GetLoginRegistrationsAsync(string userName = null);
    }
}