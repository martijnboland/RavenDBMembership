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

            given("a user wants to create an account", delegate
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
                        var value = Unique.String("supertoolongpassword123supertoolongpassword123supertoolongpassword");

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
            });

            given("the user wants to change their password", delegate
            {
                                                                     
            });
        }

        private string GetLongStringWithUniqueStart()
        {
            return Unique.Integer.ToString() + "_12345678901234567890123456789012345678901234567890";
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
