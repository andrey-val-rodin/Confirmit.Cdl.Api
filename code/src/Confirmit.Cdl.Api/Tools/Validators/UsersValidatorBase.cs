using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Tools.Validators
{
    public abstract class UsersValidatorBase<T>
        where T : class
    {
        private const int Threshold = 3;

        private int _criticalErrorCount;
        private List<int> _ids;
        private int _current;

        public List<int> WrongIds { get; } = new List<int>();
        public bool CriticalErrorCountExceeded => WrongIds.Count >= _criticalErrorCount;
        public List<T> ValidUsers { get; } = new List<T>();
        private Dictionary<long, T> Users { get; } = new Dictionary<long, T>();

        /// <summary>
        /// Checks whether the specified users exist.
        /// After execution, property ValidUsers will contain found User-objects,
        /// and property WrongIds will contain unknown IDs. If there are too many wrong Users, process is aborted,
        /// and property CriticalErrorCountExceeded returns true.
        /// Method uses two strategies. If number of users is small, method attempts to obtain users
        /// individually. If number of users exceeds threshold, method loads all users
        /// from organization of the first valid user. Thus, this strategy works great without additional requests,
        /// if only the specified users belong to one organization or at least only a few organization.
        /// </summary>
        /// <param name="ids">User IDs to check</param>
        /// <param name="criticalErrorCount">Number of errors after which execution will be aborted</param>
        public async Task ValidateAsync(IEnumerable<int> ids, int criticalErrorCount = 1)
        {
            _criticalErrorCount = criticalErrorCount;
            _ids = ids?.ToList() ?? throw new ArgumentNullException(nameof(ids));

            // Get first User
            var user = await GetFirstValidUserAsync();
            if (user == null)
                return;

            // Choose strategy based on how many users are left
            _current++;
            if (_ids.Count - _current > Threshold)
            {
                // Obtain all users in organization and keep them in dictionary
                // Try to find each next user in this dictionary without request to service
                await AddUsersFromOrganization(user);
                for (; _current < _ids.Count; _current++)
                {
                    int id = _ids[_current];
                    if (Users.ContainsKey(id))
                    {
                        var u = Users[id];
                        AddUser(id, u);
                    }
                    else
                    {
                        var u = await GetUserAsync(id);
                        if (!IsUserActive(u))
                            continue;

                        AddUser(id, u);
                        if (u == null)
                        {
                            if (CriticalErrorCountExceeded)
                                return;
                        }
                        else
                        {
                            await AddUsersFromOrganization(u);
                        }
                    }
                }
            }
            else
            {
                // Obtain Users individually
                for (; _current < _ids.Count; _current++)
                {
                    int id = _ids[_current];
                    var u = await GetUserAsync(id);
                    if (!IsUserActive(u))
                        continue;

                    AddUser(id, u);
                    if (CriticalErrorCountExceeded)
                        return;
                }
            }
        }

        private async Task AddUsersFromOrganization(T user)
        {
            // Load all users from the organization of the specified user
            var endusers = await GetUsersInOrganizationAsync(user);
            foreach (var u in endusers)
            {
                Users.Add(GetUserId(u), u);
            }
        }

        protected abstract bool IsUserActive(T user);

        protected abstract Task<T[]> GetUsersInOrganizationAsync(T user);

        protected abstract int GetUserId(T user);

        private async Task<T> GetFirstValidUserAsync()
        {
            for (_current = 0; _current < _ids.Count; _current++)
            {
                int id = _ids[_current];
                var user = await GetUserAsync(id);
                if (!IsUserActive(user))
                    continue;

                AddUser(id, user);
                if (user != null)
                    return user;

                if (CriticalErrorCountExceeded)
                    return null;
            }

            return null;
        }

        private void AddUser(int id, T user)
        {
            if (user == null)
                WrongIds.Add(id);
            else
                ValidUsers.Add(user);
        }

        protected abstract Task<T> GetUserAsync(int id);
    }
}