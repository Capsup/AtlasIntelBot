using System;
using System.Collections.Generic;
using System.Text;

namespace IntelBot.Database
{
    public interface IDatabaseFactory
    {
        //IDatabase Create( string dbName, string dbUser, string dbPassword );
        IDatabase Create( string dbName, string dbUser, string dbPassword, string dbAddress, int dbPort );
    }
}
