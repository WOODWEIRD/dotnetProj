using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController] //send a req from angular client 
[Route("api/[controller]")] //to hit this controller
public class BaseApiController : ControllerBase
{
}
