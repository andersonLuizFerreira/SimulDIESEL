using System;
using SimulDIESEL.DAL.Database;
using SimulDIESEL.DAL.Repositories;

namespace SimulDIESEL.BLL.Services.Database
{
    public sealed class LocalDatabaseService
    {
        private readonly IDatabaseInitializer _databaseInitializer;
        private readonly IModuleProfileRepository _moduleProfileRepository;

        public LocalDatabaseService()
            : this(CreateDefaultInitializer(), CreateDefaultRepository())
        {
        }

        public LocalDatabaseService(IDatabaseInitializer databaseInitializer, IModuleProfileRepository moduleProfileRepository)
        {
            _databaseInitializer = databaseInitializer ?? throw new ArgumentNullException(nameof(databaseInitializer));
            _moduleProfileRepository = moduleProfileRepository ?? throw new ArgumentNullException(nameof(moduleProfileRepository));
        }

        public LocalDatabaseStatus Initialize()
        {
            var result = _databaseInitializer.Initialize();
            var profileCount = _moduleProfileRepository.CountProfiles();
            return new LocalDatabaseStatus(result.DatabasePath, result.AppliedMigrations, profileCount);
        }

        private static IDatabaseInitializer CreateDefaultInitializer()
        {
            return ModuleDatabaseRuntimeFactory.CreateInitializer();
        }

        private static IModuleProfileRepository CreateDefaultRepository()
        {
            return CreateDefaultModuleProfileRepository();
        }

        public static IModuleProfileRepository CreateDefaultModuleProfileRepository()
        {
            return ModuleDatabaseRuntimeFactory.CreateModuleProfileRepository();
        }

        public static ModuleProfileService CreateDefaultModuleProfileService()
        {
            return new ModuleProfileService(CreateDefaultModuleProfileRepository());
        }

        public static IJ1939ReferenceCatalogRepository CreateDefaultJ1939ReferenceCatalogRepository()
        {
            return ModuleDatabaseRuntimeFactory.CreateJ1939ReferenceCatalogRepository();
        }

        public static J1939ReferenceCatalogService CreateDefaultJ1939ReferenceCatalogService()
        {
            return new J1939ReferenceCatalogService(CreateDefaultJ1939ReferenceCatalogRepository());
        }
    }
}
