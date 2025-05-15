using ChatMentor.Backend.Core.Interfaces;
using ChatMentor.Backend.Core.Interfaces.UserChatFeature;
using ChatMentor.Backend.DTOs;
using ChatMentor.Backend.Model.UserChat_Models;

namespace ChatMentor.Backend.Core.Services;

public class UserConnectionService
{
    private readonly IUserConnectionsRepository _userConnectionsRepository;
    private readonly IUserRepository _userRepository;

    public UserConnectionService(IUserConnectionsRepository userConnectionsRepository, IUserRepository userRepository)
    {
        _userConnectionsRepository = userConnectionsRepository;
        _userRepository = userRepository;
    }

    public async Task AddUserConnectionAsync(UserConnectionRequest request)
    {
        var connection = new UserConnection
        {
            UserId = await GetUserIdFromGuidAsync(request.UserId),
            ConnectionId = request.ConnectionId,
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress,
            ConnectedAt = DateTime.UtcNow
        };

        await _userConnectionsRepository.AddConnectionAsync(connection);
    }

    public async Task RemoveUserConnectionAsync(string connectionId)
    {
        Console.WriteLine(connectionId);
        await _userConnectionsRepository.RemoveConnectionAsync(connectionId);
    }

    public async Task<List<UserConnectionResponse>> GetAllConnectionsAsync()
    {
        // Fetch connections from the repository
        var connections = await _userConnectionsRepository.GetAllConnectionsAsync();

        // Map to DTO
        var connectionDtos = connections.Select(connection => new UserConnectionResponse
        {
            UserId = connection.CreatedBy.ToString(), // Assuming UserId is part of the UserConnection model
        }).ToList();

        return connectionDtos;
    }

    private async Task<int> GetUserIdFromGuidAsync(string userIdGuid)
    {
        var user = await _userRepository.GetUserByGuidAsync(userIdGuid);
        if (user == null) throw new Exception("User not found");

        return user.Id; // Assuming the user object has an int Id
    }
}