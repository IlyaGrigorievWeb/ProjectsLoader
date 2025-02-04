using Contracts.Interfaces;

namespace ProjectsLoader.Services
{
    public class ActiveUserCounter : IActiveUserCounter
    {
        private readonly List<string> _activeUsers = new List<string>();

        public IReadOnlyList<string> ActiveUsers => _activeUsers;

        public List<string> ActiveUser { get; }

        public void AddUser(string login)
        {
            lock (_activeUsers)
            {
                if (!_activeUsers.Contains(login))
                {
                    _activeUsers.Add(login);
                }
            }
        }

        public void RemoveUser(string login)
        {
            lock (_activeUsers)
            {
                _activeUsers.Remove(login);
            }
        }

        public List<string> GetActiveUser()
        {
            lock (_activeUsers)
            {
                return _activeUsers;
            }
        }
    }
}