using System.Collections.Generic;
using Faker;

namespace Dfc.CourseDirectory.Testing
{
    public class UniqueIdHelper
    {
        private readonly HashSet<int> _providerUkprns = new HashSet<int>();
        private readonly HashSet<string> _userEmails = new HashSet<string>();
        private readonly HashSet<string> _userIds = new HashSet<string>();

        public int GenerateProviderUkprn()
        {
            int ukprn;

            lock (_providerUkprns)
            {
                do
                {
                    ukprn = RandomNumber.Next(1000000, 9999999);
                }
                while (!_providerUkprns.Add(ukprn));
            }

            return ukprn;
        }

        public string GenerateUserEmail()
        {
            string email;

            lock (_userEmails)
            {
                do
                {
                    email = Internet.Email();
                }
                while (!_userEmails.Add(email));
            }

            return email;
        }

        public string GenerateUserId()
        {
            string userId;

            lock (_userIds)
            {
                do
                {
                    userId = Internet.UserName();
                }
                while (_userIds.Add(userId));
            }

            return userId;
        }
    }
}
