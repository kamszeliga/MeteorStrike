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

        //------------------------------------------------------------

        public Task AddMembersToProjectAsync(IEnumerable<string> userIds, int? projectId, int? companyId);

        public Task RemoveMembersFromProjectAsync(int? projectId, int? companyId);

        
        public Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId);

        public Task<bool> AddProjectManagerAsync(string? userId, int? projectId);

        public Task<Project> GetProjectByIdAsync(int? projectId, int? companyId);

        public Task<BTUser> GetProjectManagerAsync(int? projectId);


        public Task RemoveProjectManagerAsync(int? projectId);

        public Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId);

        public Task<IEnumerable<Project>> GetCompanyProjectsAsync (int companyId);






    }
}
