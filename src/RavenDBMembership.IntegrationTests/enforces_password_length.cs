using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using NUnit.Framework;

namespace RavenDBMembership.IntegrationTests
{
    [Explicit("test under construction")]
    public class enforces_password_length : AllProvidersSpecification
    {
        public override void SpecifyForEachProvider()
        {
            given("the configuration file has a minimum password specified", delegate
            {
                // RavenDBMembershipProvider isn't picking up the ocnfig value currently
                then("the provider loads the value");
            });

            given("a user wants to create an account", delegate
            {
                var username = Unique.String("username");
                var email = Unique.String("email") + "@someserver.com";

                when("the user chooses a password that is too short", delegate
                {
                    var password = arrange(delegate
                    {
                        var value = Unique.Integer.ToString() + "1234567890";
                        expect(() => value.Length > Membership.Provider.MinRequiredPasswordLength);
                        value = value.Substring(0, Membership.Provider.MinRequiredPasswordLength - 1);

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
                        var value = Unique.Integer.ToString() + "1234567890";
                        expect(() => value.Length > Membership.Provider.MinRequiredPasswordLength);
                        value = value.Substring(0, Membership.Provider.MinRequiredPasswordLength);

                        return value;
                    });

                    then("the user can be created", delegate
                    {
                        var user = Membership.CreateUser(username, password, email);

                        expect(() => user != null);
                    });
                });
            });
        }
    }
}
