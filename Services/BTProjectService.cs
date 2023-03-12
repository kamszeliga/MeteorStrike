using MeteorStrike.Data;
using MeteorStrike.Models;
using MeteorStrike.Models.Enums;
using MeteorStrike.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace MeteorStrike.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _btRolesService;

        public BTProjectService(ApplicationDbContext context,
                                IBTRolesService btRolesService)
        {
            _context = context;
            _btRolesService = btRolesService;
        }

        public async Task AddProjectAsync(Project project)
        {
            try
            {
                await _context.AddAsync(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                if (project != null)
                {
                    project.Archived = true;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Project> GetProjectAsync(int companyId, int projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                                .Where(p => p.CompanyId == companyId)
                                                .Include(p => p.Company)
                                                .Include(p => p.Members)
                                                .Include(p => p.ProjectPriority)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.SubmitterUser)
                                                .FirstOrDefaultAsync(m => m.Id == projectId);
                return project;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<ProjectPriority>> GetProjectPriorityAsync()
        {
            try
            {
                IEnumerable<ProjectPriority> priorities = await _context.ProjectPriorities.ToListAsync();

                return priorities;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync(int companyId)
        {
            try
            {
                IEnumerable<Project> projects = await _context.Projects
                                                    .Where(p => p.Archived == false && p.CompanyId == companyId)
                                                    .Include(p => p.Members)
                                                    .Include(p => p.ProjectPriority)
                                                    .Include(p => p.Tickets)
                                                    .ToListAsync();
                return projects;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool ProjectExists(int id)
        {
            return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IEnumerable<Project>> GetCompanyProjectsAsync(int companyId)
        {
            IEnumerable<Project> projects = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                                   .Include(p => p.ProjectPriority)
                                                                   .Include(p => p.Tickets)
                                                                   .Include(p => p.Company)
                                                                   .ToListAsync();

            return projects;

        }

        // -------------------------------------------------------------------------

        public async Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                Project project = await GetProjectByIdAsync(projectId, member!.CompanyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (!IsOnProject)
                {
                    project.Members.Add(member);

                    await _context.SaveChangesAsync();

                    return true;
                }

                return false;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddProjectManagerAsync(string? userId, int? projectId)
        {
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId);

                BTUser? selectedPM = await _context.Users.FindAsync(userId);

                //Remove current Project Manager
                if (currentPM != null)
                {
                    await RemoveProjectManagerAsync(projectId);
                }

                // Add New Project Manager
                try
                {
                    await AddMemberToProjectAsync(selectedPM!, projectId);

                    return true;
                }
                catch (Exception)
                {

                    throw;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Project> GetProjectByIdAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                                .Where(p => p.CompanyId == companyId)
                                                .Include(p => p.Company)
                                                .Include(p => p.Members)
                                                .Include(p => p.ProjectPriority)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.SubmitterUser)
                                                .FirstOrDefaultAsync(m => m.Id == projectId);
                return project;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser> GetProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser member in project!.Members)
                {
                    if (await _btRolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }

                return null!;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser member in project!.Members)
                {
                    if (await _btRolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        await RemoveMemberFromProjectAsync(member, projectId);
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, member!.CompanyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (IsOnProject)
                {
                    project.Members.Remove(member);

                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddMembersToProjectAsync(IEnumerable<string> userIds, int? projectId, int? companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, companyId);

                foreach (string userId in userIds)
                {
                    BTUser? btUser = await _context.Users.FindAsync(userId);

                    if (project != null && btUser != null)
                    {
                        bool IsOnProject = project.Members.Any(m => m.Id == userId);

                        if (!IsOnProject)
                        {
                            project.Members.Add(btUser);
                        }
                        else
                        {
                            continue;
                        }

                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveMembersFromProjectAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, companyId);

                foreach (BTUser member in project.Members)
                {
                    if (!await _btRolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        project.Members.Remove(member);
                    }
                }

                await _context.SaveChangesAsync();
            }

            catch (Exception)
            {

                throw;
            }
        }
    }
}
