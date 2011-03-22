using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using Plugins.Commands;

namespace Plugins.SqlServer
{
    [Export(typeof(ICommand))]
    public class KillAllSqlConnections : BaseCommand
    {
        public override void Act()
        {
            var sessionIds = new List<int>();
            using (var connection = new SqlConnection("Data Source=.;Integrated Security=True;"))
            {
                connection.Open();
                try
                {
                    var command =
                        new SqlCommand("select session_id from sys.dm_exec_connections where session_id != @@SPID",
                                       connection);
                    var sqlDataReader = command.ExecuteReader();
                    while (sqlDataReader.Read())
                    {
                        sessionIds.Add((int) sqlDataReader["session_id"]);
                    }
                    sqlDataReader.Close();
                    foreach (var sessionId in sessionIds)
                    {
                        try
                        {
                            using (var kill = new SqlCommand(string.Format("kill {0}", sessionId), connection))
                            {
                                kill.ExecuteNonQuery();
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public override string Description
        {
            get { return "Kills all currently connected clients to the sql server database"; }
        }
    }
}