using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using NJasmine;

namespace RavenDBMembership.IntegrationTests
{
    public class can_load_database : GivenWhenThenFixture
    {
        public override void Specify()
        {
            then("the database can be loaded", delegate
            {
                var connection = arrange(() => new SqlConnection(Properties.Settings.Default.MembershipDatabase));
                connection.Open();
                var command = arrange(() => new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", connection));

                command.ExecuteNonQuery();
            });
        }
    }
}
