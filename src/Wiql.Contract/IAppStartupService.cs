using System.Threading.Tasks;

namespace Wiql.Contract
{
    public interface IAppStartupService
    {
        Task<int> RunApp();
    }
}