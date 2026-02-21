using EmployeeDemo.Domain.Entities;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.Repositories
{
    public interface IMeetingRepository: IGenericRepository<Meeting>
    {
        // inherits AddAsync from generic; no extra methods required
    }
}
