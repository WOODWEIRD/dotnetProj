using System.Collections.Generic;
using API.Data;
using API.DTOs;
using API.Entites;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;


public class AccountController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(
        UserManager<AppUser> userManager,
        ITokenService tokenService,
        IMapper mapper
     )
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }


    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.UserName))
            return BadRequest("Username taken");
        var user = _mapper.Map<AppUser>(registerDto);

        user.UserName = registerDto.UserName.ToLower();


        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        var rolesResult = await _userManager.AddToRoleAsync(user, "Member");
        if (!rolesResult.Succeeded) return BadRequest(result.Errors);

        return new UserDto
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            knownAs = user.KnownAs,
            Gender = user.Gender
        };
    }
    private async Task<bool> UserExists(string username)
    {
        return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
    }




    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.Users.Include(p => p.Photos).SingleOrDefaultAsync(x =>
        x.UserName == loginDto.Username);
        if (user == null) return Unauthorized("invalid username");

        if (user.Photos.Count == 0)
        {
            user.Photos.Add(new Photo
            {
                Url = "https://randomuser.me/api/portraits/lego/3.jpg",
                IsMain = true
            });
        }

        var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!result) return Unauthorized("invalid password");



        return new UserDto
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            photoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
            knownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

}
