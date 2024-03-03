using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ServiceFilter(typeof(LogUserActivity))]
[ApiController] //send a req from angular client 
[Route("api/[controller]")] //to hit this controller
public class BaseApiController : ControllerBase
{
}
