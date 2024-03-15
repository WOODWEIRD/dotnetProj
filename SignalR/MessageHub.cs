using System.Security.AccessControl;
using API.DTOs;
using API.Entites;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub : Hub
{
    private readonly IMessageRepo _messageRepo;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;

    public MessageHub(
        IMessageRepo messageRepo,
        IUserRepository userRepository,
        IMapper mapper,
        IHubContext<PresenceHub> presenceHub
        )
    {
        _messageRepo = messageRepo;
        _userRepository = userRepository;
        _mapper = mapper;
        _presenceHub = presenceHub;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext.Request.Query["user"];
        var groupName = GetGroupName(Context.User.GetUserName(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroup(groupName);

        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await _messageRepo.GetMessageThread(Context.User.GetUserName(), otherUser);

        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var group = await RemoveFromGroupMessage();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var username = Context.User.GetUserName();

        if (username == createMessageDto.RecipientUsername.ToLower())
            throw new HubException("cant send msg to yourself");

        var sender = await _userRepository.GetUserByUsername(username);
        var recipient = await _userRepository.GetUserByUsername(createMessageDto.RecipientUsername);
        if (recipient == null) throw new HubException("not found user");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content,
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await _messageRepo.GetMessageGroup(groupName);
        if (group.Connections.Any(x => x.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            if (connections != null)
            {
                await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                new { username = sender.UserName, knownAs = sender.KnownAs });
            }
        }

        _messageRepo.AddMessage(message);

        if (await _messageRepo.SaveAllAsync())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
        }
    }

    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";

    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var group = await _messageRepo.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());
        if (group == null)
        {
            group = new Group(groupName);
            _messageRepo.AddGroup(group);
        }
        group.Connections.Add(connection);
        if (await _messageRepo.SaveAllAsync()) return group;

        throw new HubException("failed to add to group");

    }


    private async Task<Group> RemoveFromGroupMessage()
    {
        var group = await _messageRepo.GetGroupForConnection(Context.ConnectionId);
        var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        _messageRepo.RemoveConnection(connection);

        if (await _messageRepo.SaveAllAsync()) return group;

        throw new HubException("failed to remove from group");
    }
}
