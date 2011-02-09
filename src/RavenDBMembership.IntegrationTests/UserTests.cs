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
                var username = "martijn";
                var password = "1Password0";
                var email = "someemail@someserver.com";

                var user = arrange(() => Membership.CreateUser(username, password, email));

                then("that user can be loaded", delegate
                {
                    var loadedUser = Membership.GetUser(username);

                    expect(() => loadedUser.ProviderUserKey == user.ProviderUserKey);
                    expect(() => loadedUser.UserName == username);
                    expect(() => loadedUser.Email == email);
                });
            });
        }
    }
}