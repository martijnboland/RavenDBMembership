using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using NUnit.Framework;

namespace RavenDBMembership.IntegrationTests
{
    public abstract class enforces_password_length : SpecificationForAllMembershipProviders
    {
        public abstract int GetMinimumPasswordLength();

        public override Dictionary<string, string> GetAdditionalConfiguration()
        {
            return new Dictionary<string,string>
            {
                {"minRequiredPasswordLength", GetMinimumPasswordLength().ToString()}
            };
        }

        public override void SpecifyForEach()
        {
            given("the configuration file has a minimum password specified", delegate
            {
               then("the provider loads the value", delegate
                {
                    expect(() => Membership.Provider.MinRequiredPasswordLength == GetMinimumPasswordLength());
                });
            });

            given("a user", delegate
            {
                var username = Unique.String("username");
                var email = Unique.String("email") + "@someserver.com";

                when("the user chooses a password that is too short", delegate
                {
                    var password = arrange(delegate
                    {
                        var value = GetLongStringWithUniqueStart();
                        expect(() => value.Length > GetMinimumPasswordLength());
                        value = value.Substring(0, GetMinimumPasswordLength() - 1);

                        return value;
                    });

                    then("an exception is thrown", delegate
                    {
                        var exception = Assert.Throws<MembershipCreateUserException>(delegate
                        {
                            Membership.CreateUser(username, password, email);
                        });

                        expect(() => exception.StatusCode == MembershipCreateStatus.InvalidPassword);
                    });
                });

                when("the user chooses a password that is very long (is there a configurable limit?)", delegate
                {
                    var password = arrange(delegate
                    {
                        var value = GetLongStringWithUniqueStart();

                        return value;
                    });

                    then("the user can be created", delegate
                    {
                        var user = Membership.CreateUser(username, password, email);

                        expect(() => user != null);
                    });
                });

                when("the user chooses a password that is long enough", delegate
                {
                    var password = arrange(delegate
                    {
                        var value = GetLongStringWithUniqueStart();
                        expect(() => value.Length > GetMinimumPasswordLength());
                        value = value.Substring(0, GetMinimumPasswordLength());

                        return value;
                    });

                    then("the user can be created", delegate
                    {
                        var user = Membership.CreateUser(username, password, email);

                        expect(() => user != null);
                    });
                });

                given("the user already has an account", delegate
                {
                    var password =
                        GetLongStringWithUniqueStart().Substring(
                            Membership.Provider.MinRequiredPasswordLength);

                    var existingUser = Membership.CreateUser(username, password, email);

                    when("the user tries to change their password to something too short", delegate
                    {
                        var newPassword =
                            GetLongStringWithUniqueStart().Substring(Membership.Provider.MinRequiredPasswordLength - 1);

                        then("an exception is thrown", delegate
                        {
                            var exception = Assert.Throws<ArgumentException>(delegate
                            {
                                bool result = existingUser.ChangePassword(password, newPassword);
                            });

                            expect(() => exception.ParamName.Equals("newPassword"));
                            expect(() => exception.Message.Contains("too long"));
                        });
                    });

                    when("the user changes their password to something reasonable", delegate
                    {
                        var newPassword =
                            GetLongStringWithUniqueStart().Substring(Membership.Provider.MinRequiredPasswordLength);

                        bool result = arrange(() => existingUser.ChangePassword(password, newPassword));

                        then("the save succeeds", delegate
                        {
                            expect(() => result == true);
                        });

                        then("the user can log in with their new password", delegate
                        {
                            expect(() => Membership.ValidateUser(username, newPassword));
                        });

                        then("the user cannot log in with their old password", delegate
                        {
                            expect(() => !Membership.ValidateUser(username, password));
                        });
                    });
                });
            });
        }

        private string GetLongStringWithUniqueStart()
        {
            return Unique.Integer.ToString() + "_12345678901234567890123456789012345678901234567890"
+ "_12345678901234567890123456789012345678901234567890_12345678901234567890123456789012345678901234567890";
        }
    }

    public class enforces_password_length_of_16 : enforces_password_length
    {
        public override int GetMinimumPasswordLength()
        {
            return 16;
        }
    }

    public class enforces_password_length_of_4 : enforces_password_length
    {
        public override int GetMinimumPasswordLength()
        {
            return 4;
        }
    }
}
