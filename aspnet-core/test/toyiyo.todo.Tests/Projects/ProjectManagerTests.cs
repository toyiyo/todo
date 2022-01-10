using System;
using System.Threading.Tasks;
using toyiyo.todo.Projects;
using toyiyo.todo.Sessions;
using Xunit;
using Shouldly;

namespace toyiyo.todo.Tests.Projects
{
    public class ProjectManagerTests : todoTestBase
    {
        private readonly IProjectManager _projectManager;
        private readonly ISessionAppService _sessionAppService;
        public ProjectManagerTests()
        {
            _projectManager = Resolve<IProjectManager>();
            _sessionAppService = Resolve<ISessionAppService>();
        }
        [Fact]
        public async Task CreateProject_ReturnsNewProject()
        {
            // Arrange
            await _sessionAppService.GetCurrentLoginInformations();
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();

            // Act
            var project = Project.Create("test", currentUser, currentTenant.Id);
            var result = await _projectManager.Create(project);
            
            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe("test");
        }
    }
}
