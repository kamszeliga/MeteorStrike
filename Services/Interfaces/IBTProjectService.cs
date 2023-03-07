using MeteorStrike.Models;

namespace MeteorStrike.Services.Interfaces
{
    public interface IBTProjectService
    {

        public Task AddProjectAsync(Project project);

        public Task<Project> GetProjectAsync(int companyId, int projectId);

        public Task<IEnumerable<Project>> GetProjectsAsync(int companyId);

        public Task ArchiveProjectAsync(Project project);

        public Task UpdateProjectAsync(Project project);

        //-----------------------------------------------------------

        public Task<IEnumerable<ProjectPriority>> GetProjectPriorityAsync();

        public bool ProjectExists(int projectId);


    }
}
