namespace Contracts.Interfaces;

public interface IActiveUserCounter
{
    List<string> ActiveUser { get; }
    void AddUser(string username);
    void RemoveUser(string username);
    List<string> GetActiveUser();
}