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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MessagesController(
        IUnitOfWork unitOfWork,
        IMapper mapper
    )
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUserName();
        if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("cant send msg to urself");
        var sender = await _unitOfWork.UserRepository.GetUserByUsername(username);
        var recipient = await _unitOfWork.UserRepository.GetUserByUsername(createMessageDto.RecipientUsername);
        if (recipient == null) return NotFound();

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content,
        };

        _unitOfWork.MessageRepository.AddMessage(message);
        if (await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDto>(message));
        return BadRequest("failed to send msg");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUserName();

        var messages = await _unitOfWork.MessageRepository.GetMessageForUser(messageParams);

        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

        return messages;

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUserName();
        var message = await _unitOfWork.MessageRepository.GetMessage(id);
        if (message.SenderUsername != username && message.RecipientUsername != username)
            return Unauthorized();
        if (message.SenderUsername == username) message.senderDeleted = true;
        if (message.RecipientUsername == username) message.RecipientDeleted = true;
        if (message.RecipientDeleted && message.senderDeleted)
        {
            _unitOfWork.MessageRepository.DeleteMessage(message);
        }

        if (await _unitOfWork.Complete()) return Ok();

        return BadRequest("problem deleting the msg");
    }
}

