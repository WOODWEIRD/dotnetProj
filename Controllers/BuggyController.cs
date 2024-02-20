using API.Data;
using API.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BuggyController(DataContext _context) : BaseApiController
{
    private readonly DataContext _context = _context;

    [Authorize]
    [HttpGet("auth")]
    public ActionResult<String> getSercret()
    {
        return "secret text";
    }


    [HttpGet("not-found")]
    public ActionResult<AppUser> getNotFound()
    {
        var thing = _context.Users.Find(-1);
        if (thing == null) return NotFound();
        return thing;
    }


    [HttpGet("server-error")]
    public ActionResult<String> getServerError()
    {
        var thing = _context.Users.Find(-1);
        return thing.ToString();
    }


    [HttpGet("bad-request")]
    public ActionResult<String> getBadRequest()
    {
        return BadRequest("bad req 400 error");
    }
}
