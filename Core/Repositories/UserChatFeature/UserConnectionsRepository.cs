using ChatMentor.Backend.Core.Interfaces.UserChatFeature;
using ChatMentor.Backend.Data;
using ChatMentor.Backend.Model.UserChat_Models;
using Microsoft.EntityFrameworkCore;

namespace ChatMentor.Backend.Core.Repositories.UserChatFeature;

public class UserConnectionsRepository: IUserConnectionsRepository
{
    private readonly ChatMentorDbContext _context;

    public UserConnectionsRepository(ChatMentorDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserConnection>> GetAllConnectionsAsync()
    {
        return await _context.TblUserConnections.ToListAsync();
    }

    public async Task<UserConnection> AddConnectionAsync(UserConnection connection)
    {
        await _context.TblUserConnections.AddAsync(connection);
        await _context.SaveChangesAsync();
        return connection;
    }

    public async Task<bool> RemoveConnectionAsync(string connectionId)
    {
        var conn = await _context.TblUserConnections.FirstOrDefaultAsync(c => c.ConnectionId == connectionId);
        if (conn != null)
        {
            _context.TblUserConnections.Remove(conn);
            await _context.SaveChangesAsync();
        }
        return conn != null;
    }
}