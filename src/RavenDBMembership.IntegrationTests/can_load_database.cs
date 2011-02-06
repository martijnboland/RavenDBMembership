using System.IO;
using System.Linq;
using NJasmine;
using NUnit.Framework;

namespace RavenDBMembership.IntegrationTests
{
    public class can_load_database : GivenWhenThenFixture
    {
        public override void Specify()
        {
            var databaseTempDirectory = Path.Combine(@"c:\temp", "RavenDBMembershipTest");
            var secondPath = Path.Combine(databaseTempDirectory, @"TestSqlMembership.mdf");

            arrange(delegate
            {
                if (!Directory.Exists(databaseTempDirectory))
                    Directory.CreateDirectory(databaseTempDirectory);
            });
            
            then("the database scripts can be loaded", delegate
            {
                var createScripts = DatabaseInitialization.GetCreateScript();

                Assert.That(createScripts.Any(s => s.Contains("TABLE")), Is.True);
                Assert.That(createScripts.Count(), Is.GreaterThan(5));
            });

            then("the database can be generated", delegate
            {
                string databaseName = "SqlMembership";

                DatabaseInitialization.RecreateDatabase(databaseName, secondPath);

                DatabaseInitialization.RunSqlMembershipCreationScript(databaseName);
            });
        }
    }
}
