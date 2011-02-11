using System;
using System.Web.Security;

namespace RavenDBMembership.IntegrationTests
{
    public class UserTests : AllProvidersSpecification
    {
        public override void SpecifyForEachProvider()
        {
            when("a user has been created", delegate
            {
                var username = "martijn" + Unique.Integer;
                var password = "1Password0";
                var email = "someemail" + Unique.Integer + "@someserver.com";

                var user = arrange(() => Membership.CreateUser(username, password, email));

                then("that user can be loaded", delegate
                {
                    var loadedUser = Membership.GetUser(username);

                    expect(() => loadedUser.ProviderUserKey.Equals(user.ProviderUserKey));
                    expect(() => loadedUser.UserName.Equals(username));
                    expect(() => loadedUser.Email.Equals(email));
                });

                then("the user can log in", delegate
                {
                    expect(() => Membership.Provider.ValidateUser(username, password));
                });

                then("the user can log in if their username has extra whitespace (SqlMembershipProvider compatibility)", delegate
                {
                    expect(() => Membership.Provider.ValidateUser(username + " ", password));
                });

                then("the user can log in if their password has extra whitespace (SqlMembershipProvider compatibility)", delegate
                {
                    expect(() => Membership.Provider.ValidateUser(username, password + " "));
                });

                then("the user can't log in with the wrong password", delegate
                {
                    expect(() => !Membership.Provider.ValidateUser(username, password + "P"));
                });
            });
        }
    }
}