using API.DTOs;
using API.Entites;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepo _messageRepo;
    private readonly IMapper _mapper;

    public MessagesController(
        IUserRepository userRepository,
        IMessageRepo messageRepo,
        IMapper mapper
    )
    {
        _userRepository = userRepository;
        _messageRepo = messageRepo;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUserName();
        if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("cant send msg to urself");
        var sender = await _userRepository.GetUserByUsername(username);
        var recipient = await _userRepository.GetUserByUsername(createMessageDto.RecipientUsername);
        if (recipient == null) return NotFound();

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content,
        };

        _messageRepo.AddMessage(message);
        if (await _messageRepo.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));
        return BadRequest("failed to send msg");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUserName();

        var messages = await _messageRepo.GetMessageForUser(messageParams);

        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

        return messages;

    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUserName();

        return Ok(await _messageRepo.GetMessageThread(currentUsername, username));

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUserName();
        var message = await _messageRepo.GetMessage(id);
        if (message.SenderUsername != username && message.RecipientUsername != username)
            return Unauthorized();
        if (message.SenderUsername == username) message.senderDeleted = true;
        if (message.RecipientUsername == username) message.RecipientDeleted = true;
        if (message.RecipientDeleted && message.senderDeleted)
        {
            _messageRepo.DeleteMessage(message);
        }
                        
        if (await _messageRepo.SaveAllAsync()) return Ok();

        return BadRequest("problem deleting the msg");
    }
}

