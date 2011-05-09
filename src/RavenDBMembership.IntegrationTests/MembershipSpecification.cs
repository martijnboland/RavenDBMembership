using System.Collections.Generic;
using NJasmine;
using RavenDBMembership.IntegrationTests.ProviderFixtures;

namespace RavenDBMembership.IntegrationTests
{
    public abstract class MembershipSpecification : GivenWhenThenFixture
    {
        public virtual Dictionary<string, string> GetAdditionalConfiguration()
        {
            return new Dictionary<string, string>();
        }

        public void arrange_membership_provider(MembershipProviderOverride membership)
        {
            var gwt = new GivenWhenThenContext(_skeleFixture);

            gwt.beforeAll(delegate
                              {
                                  membership.InjectMembershipImplementation(GetAdditionalConfiguration());
                              });

            gwt.afterAll(() => membership.RestoreMembershipImplementation());
        }
    }
}