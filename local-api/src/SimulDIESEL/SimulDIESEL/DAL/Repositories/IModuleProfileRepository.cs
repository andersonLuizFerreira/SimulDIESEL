using System.Collections.Generic;
using SimulDIESEL.DTL.Modules;

namespace SimulDIESEL.DAL.Repositories
{
    public interface IModuleProfileRepository
    {
        void Create(ModuleProfileDto profile);

        ModuleProfileDto GetById(string id);

        bool Update(ModuleProfileDto profile);

        bool SoftDelete(string id, string deletedAt, string syncStatus);

        IReadOnlyList<ModuleProfileDto> ListActive();

        int CountProfiles();
    }
}
