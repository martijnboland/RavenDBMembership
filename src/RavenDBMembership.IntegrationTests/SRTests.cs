using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NJasmine;
using RavenDBMembership.UserStrings;

namespace RavenDBMembership.IntegrationTests
{
    public class SRTests : GivenWhenThenFixture
    {
        public override void Specify()
        {
            given("a string resouce", delegate
            {
                var resource = SR.sample_string_resource_2;
               
                then("the resource can be used to generate a user string", delegate
                {
                    var result = resource.WithParameters(1, 2);

                    expect(() => result.Equals("A sample user friendly string with values 1 and 2."));
                });               
            });
        }
    }
}
