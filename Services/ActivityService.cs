using DatingApp.API.Entities;
using System.Threading.Tasks;

namespace DatingApp.API.Services
{
    #region Interface
    public interface IActivityService
    {
        Task Create(Activity model);
    }
    #endregion
    public class ActivityService: IActivityService
    {
        public Task Create(Activity model)
        {
            throw new System.NotImplementedException();
        }
    }
}
