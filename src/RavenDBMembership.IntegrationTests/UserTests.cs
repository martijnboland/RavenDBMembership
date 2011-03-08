using System;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Security;
using NUnit.Framework;
using RavenDBMembership.Provider;

namespace RavenDBMembership.IntegrationTests
{
    public class UserTests : SpecificationForAllMembershipProviders
    {
        public override void SpecifyForEach(bool testingOriginalMembershipProvider)
        {
            given("a user", delegate
            {
                var username = Unique.String("username");
                var password = Unique.String("password");
                var email = Unique.String("email") + "@someserver.com";

                when("that user is created", delegate
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

                    foreach(var similarEmail in new [] { email.ToLower(), email.ToUpper()})
                    {
                        when("another user tries to change their email to be similar", delegate
                        {
                            string otherUsername = Unique.String("username");
                            string otherPassword = Unique.String("password");
                            string otherEmail = Unique.String("email");

                            arrange(() => Membership.CreateUser(otherUsername, otherPassword, otherEmail));

                            then("an exception is thrown", delegate
                            {
                                var otherUser = Membership.GetUser(otherUsername);
                                otherUser.Email = similarEmail;

                                var exception = Assert.Throws<ProviderException>(delegate
                                {
                                    Membership.UpdateUser(otherUser);
                                });

                                expect(() => exception.Message.Equals("The E-mail supplied is invalid."));
                            });
                        });
                    }
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

                given("a user that has been deleted", delegate
                {
                    var user = arrange(() => Membership.CreateUser(username, password, email));
                    
                    arrange(() => expect(() => Membership.DeleteUser(username)));

                    var otherUsername = Unique.String("username");
                    var otherPassword = Unique.String("password");
                    var otherEmail = Unique.String("email") + "@anotherServer.com";

                    when("another user is created with the same username", delegate
                    {
                        otherUsername = username;

                        then("that new user can be created", delegate
                        {
                            Membership.CreateUser(otherUsername, otherPassword, otherEmail);
                        });
                    });

                    when("another user is created with the same email", delegate
                    {
                        otherEmail = email;

                        then("that new user can be created", delegate
                        {
                            Membership.CreateUser(otherUsername, otherPassword, otherEmail);
                        });
                    });
                });
            });
        }
    }
}