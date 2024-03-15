using API.DTOs;
using API.Entites;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepo : IMessageRepo
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessageRepo(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void AddGroup(Group group)
    {
        _context.Groups.Add(group);
    }

    public void AddMessage(Message message)
    {
        _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        _context.Messages.Remove(message);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
        return await _context.Connections.FindAsync(connectionId);
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
        return await _context.Groups
         .Include(x => x.Connections)
         .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
         .FirstOrDefaultAsync();
    }

    public async Task<Message> GetMessage(int id)
    {
        return await _context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
    {
        var query = _context.Messages
        .OrderByDescending(x => x.MessageSent)
        .AsQueryable();

        query = messageParams.Container switch
        {
            //view where recipient is me
            "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username
                && u.RecipientDeleted == false),

            //view where sender is me
            "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username
                && u.senderDeleted == false),

            //view where recipient is me and messages are unread
            _ => query.Where(u => u.RecipientUsername == messageParams.Username
                    && u.DateRead == null && u.RecipientDeleted == false)
        };

        var message = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

        return await PagedList<MessageDto>
            .CreateAsync(message, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<Group> GetMessageGroup(string groupName)
    {
        return await _context.Groups
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string RecepientUsername)
    {
        var messages = await _context.Messages
            .Include(u => u.Sender).ThenInclude(p => p.Photos)
            .Include(u => u.Recipient).ThenInclude(p => p.Photos)
            .Where(
                n => n.RecipientUsername == currentUsername
                && n.SenderUsername == RecepientUsername
                && n.RecipientDeleted == false
                ||
                n.RecipientUsername == RecepientUsername
                && n.SenderUsername == currentUsername
                && n.senderDeleted == false
            ).OrderBy(m => m.MessageSent)
            .ToListAsync();
        var unreadMessages = messages.Where(m => m.DateRead == null
        && m.RecipientUsername == currentUsername).ToList();

        if (unreadMessages.Count != 0)
        {
            foreach (var message in unreadMessages)
            {
                message.DateRead = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }
        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public void RemoveConnection(Connection connection)
    {
        _context.Connections.Remove(connection);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
