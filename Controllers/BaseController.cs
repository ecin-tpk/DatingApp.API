using DatingApp.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Controller]
    public class BaseController: ControllerBase
    {
        // Logged in user is added to HttpContext.Items by JwtMiddleware
        public User User => (User)HttpContext.Items["User"];
    }
}
