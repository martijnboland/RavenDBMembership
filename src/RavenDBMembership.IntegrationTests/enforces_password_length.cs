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

        public override void SpecifyForEach(bool usingOriginalMembershipProvider)
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
                    var password = arrange(() => GetUniqueishStringWithLength(GetMinimumPasswordLength() - 1));

                    then("an exception is thrown", delegate
                    {
                        var exception = Assert.Throws<MembershipCreateUserException>(delegate
                        {
                            Membership.CreateUser(username, password, email);
                        });

                        expect(() => exception.StatusCode == MembershipCreateStatus.InvalidPassword);
                    });
                });

                when("the user chooses a password that is very long (SQLMembershipProvider has a limit at about ~0x80)", delegate
                {
                    var password = arrange(() => GetUniqueishStringWithLength(0x70));

                    then("the user can be created", delegate
                    {
                        var user = Membership.CreateUser(username, password, email);
                        
                        expect(() => user != null);
                    });
                });

                when("the user chooses a password that is long enough", delegate
                {
                    var password = arrange(() => GetUniqueishStringWithLength(GetMinimumPasswordLength()));

                    then("the user can be created", delegate
                    {
                        var user = Membership.CreateUser(username, password, email);

                        expect(() => user != null);
                    });
                });

                given("the user already has an account", delegate
                {
                    var password = arrange(() => GetUniqueishStringWithLength(Membership.Provider.MinRequiredPasswordLength));

                    var existingUser = arrange(() => Membership.CreateUser(username, password, email));

                    when("the user tries to change their password via MembershipProvider.ChangePassword()", delegate
                    {
                    });

                    when("the user tries to change their password via MembershipUser.ChangePassword()", delegate
                    {
                        when("the user tries to change their password to something too short", delegate
                        {
                            if (usingOriginalMembershipProvider)
                            {
                                ignoreBecause("The original SqlMembershipProvider does not check password length on update.");
                            }

                            var newPassword = arrange(() => GetUniqueishStringWithLength(Membership.Provider.MinRequiredPasswordLength - 1));

                            then("an exception is thrown", delegate
                            {
                                var exception = Assert.Throws<ArgumentException>(delegate
                                {
                                    bool result = existingUser.ChangePassword(password, newPassword);
                                });

                                expect(() => exception.ParamName == "newPassword");
                                expect(() => exception.Message.Contains("Password is shorter than the minimum"));
                            });
                        });

                        when("the user changes their password to something reasonable", delegate
                        {
                            var newPassword = arrange(() => GetUniqueishStringWithLength(Membership.Provider.MinRequiredPasswordLength));

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
                                expect(() => newPassword != password);
                                expect(() => !Membership.ValidateUser(username, password));
                            });
                        });

                    });
                });
            });
        }

        public override Dictionary<string, string> GetAdditionalConfiguration()
        {
            return new Dictionary<string, string>
            {
                {"minRequiredPasswordLength", GetMinimumPasswordLength().ToString()}
            };
        }

        private string GetUniqueishStringWithLength(int length)
        {
            string longStringWithUniqueStart = Unique.Integer.ToString();

            while (longStringWithUniqueStart.Length < length)
                longStringWithUniqueStart += "_12345678901234567890123456789012345678901234567890";

            return longStringWithUniqueStart.Substring(0, length);
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
