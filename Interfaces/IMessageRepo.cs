using API.DTOs;
using API.Entites;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepo
{
    void AddMessage(Message message);
    void DeleteMessage(Message message);
    Task<Message> GetMessage(int id);
    Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams);
    Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string RecepientUsername);
    Task<bool> SaveAllAsync();
}
