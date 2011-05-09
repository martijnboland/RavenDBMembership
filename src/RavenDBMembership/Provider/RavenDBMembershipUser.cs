using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace RavenDBMembership.Provider
{
    class RavenDBMembershipUser : MembershipUser
    {
        public RavenDBMembershipUser(string providerName, string username, string id, string email, string passwordQuestion, string comment, bool isApproved, bool isLockedOut, DateTime creationDate, DateTime lastLoginDate, DateTime lastActivityDate, DateTime lastPasswordChangedDate, DateTime lastLockoutDate)
            :base (providerName, username, id, email, passwordQuestion, comment, isApproved, isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, lastLockoutDate)
        {
        }

        public override bool ChangePassword(string oldPassword, string newPassword)
        {
            return Membership.Providers[ProviderName].ChangePassword(UserName, oldPassword, newPassword);
        }
    }
}
