namespace AlertBot.WebApi.Models;

public interface IUserConfigurationStore
{
    Task CreateAsync(UserConfiguration configuration);

    Task<UserConfiguration> ReadAsync(long id);

    Task UpdateAsync(UserConfiguration configuration);

    Task DeleteAsync(long id);
}