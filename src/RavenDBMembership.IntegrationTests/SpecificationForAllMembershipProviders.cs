using System;
using System.Linq;
using System.Text;
using RavenDBMembership.IntegrationTests.ProviderFixtures;

namespace RavenDBMembership.IntegrationTests
{
    public abstract class SpecificationForAllMembershipProviders : MembershipSpecification
    {
        public abstract void SpecifyForEach();

        public sealed override void Specify()
        {
            when("using RavenMembershipProvider in-memory", delegate
            {
                var provider = new OverrideForInMemoryRavenMembershipProvider();

                arrange_membership_provider(provider);

                SpecifyForEach();
            });

            when("using SQLMembershipProvider", delegate
            {
                var provider = new OverrideForSqlMembershipProvider();

                arrange_membership_provider(provider);

                SpecifyForEach();
            });

            when("using raven with munin on disk", delegate
            {
                var provider = new OverrideForMuninRavenMembershipProvider();

                arrange_membership_provider(provider);

                SpecifyForEach();
            });

            when("using raven with esent on disk", delegate
            {
                var provider = new OverrideForEsentRavenMembershipProvider();

                arrange_membership_provider(provider);

                SpecifyForEach();
            });
        }
    }
}
