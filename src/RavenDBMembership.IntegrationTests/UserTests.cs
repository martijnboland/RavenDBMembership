using System;
using System.Web.Security;
using NUnit.Framework;

namespace RavenDBMembership.IntegrationTests
{
    public class UserTests : AllProvidersSpecification
    {
        public override void SpecifyForEachProvider()
        {
            given("a user", delegate
            {
                var username = Unique.String("username");
                var password = Unique.String("password");
                var email = Unique.String("email") + "@someserver.com";

                when("created", delegate
                {
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

                    when("another user wants to register with the same username", delegate
                    {
                        string otherPassword = Unique.String("password");
                        string otherEmail = Unique.String("password") + "@someserver.com";

                        then("an excepton is thrown", delegate
                        {
                            var exception = Assert.Throws<MembershipCreateUserException>(() => Membership.CreateUser(username, otherPassword, otherEmail));

                            expect(() => exception.StatusCode == MembershipCreateStatus.DuplicateUserName);
                        });
                    });

                    when("another user wants to register with the same email", delegate
                    {
                        string otherUsername = Unique.String("username");
                        string otherPassword = Unique.String("password");

                        then("an excepton is thrown", delegate
                        {
                            var exception = Assert.Throws<MembershipCreateUserException>(() => Membership.CreateUser(otherUsername, otherPassword, email));

                            expect(() => exception.StatusCode == MembershipCreateStatus.DuplicateEmail);
                        });
                    });
                });

                when("created with whitespace in the username and password", delegate
                {
                    username = username + " ";
                    password = password + " ";

                    var user = arrange(() => Membership.CreateUser(username, password, email));

                    then("the username does not actually include the whitespace", delegate
                    {
                        var loadedUser = Membership.GetUser(username);

                        expect(() => loadedUser.UserName == username.Trim());
                        expect(() => loadedUser.UserName == user.UserName);
                    });

                    then("the password does not actually include the whitespace", delegate
                    {
                        expect(() => Membership.Provider.ValidateUser(username, password.Trim()));
                    });
                });
            });
        }
    }
}