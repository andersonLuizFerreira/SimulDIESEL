using SimulDIESEL.DAL.Repositories;

namespace SimulDIESEL.DAL.Database
{
    public static class ModuleDatabaseRuntimeFactory
    {
        public static IDatabaseInitializer CreateInitializer()
        {
            var connectionFactory = new SqliteConnectionFactory();
            return new SqliteDatabaseInitializer(connectionFactory, new SqliteMigrationRunner());
        }

        public static IModuleProfileRepository CreateModuleProfileRepository()
        {
            var connectionFactory = new SqliteConnectionFactory();
            var provider = new SqliteBdServiceProvider(connectionFactory);
            return new SqliteModuleProfileRepository(provider);
        }
    }
}
