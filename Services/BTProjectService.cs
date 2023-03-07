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

        public BTProjectService(ApplicationDbContext context)
        {
            _context = context;
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
                    project.Archived= true;
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
            }        }

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
    }
}
