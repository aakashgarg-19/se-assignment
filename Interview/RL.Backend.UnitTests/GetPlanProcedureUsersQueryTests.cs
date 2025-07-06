using FluentAssertions;

using Moq;

using RL.Backend.Commands;
using RL.Backend.Commands.Handlers.PlanProcedure;
using RL.Backend.Exceptions;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.UnitTests;

[TestClass]
public class GetPlanProcedureUsersQueryTests
{
    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    public async Task GetPlanProcedureUsersQuery_InvalidPlanProcedureId_ReturnsBadRequest(int planProcedureId)
    {
        // Arrange
        var context = new Mock<RLContext>();
        var sut = new GetPlanProcedureUsersQueryHandler(context.Object);

        var request = new GetPlanProcedureUsersQuery
        {
            PlanProcedureId = planProcedureId
        };

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        result.Exception.Should().BeOfType<BadRequestException>();
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    public async Task GetPlanProcedureUsersQuery_NoUsersFound_ReturnsEmptyList()
    {
        // Arrange
        var context = DbContextHelper.CreateContext();
        var sut = new GetPlanProcedureUsersQueryHandler(context);

        var request = new GetPlanProcedureUsersQuery
        {
            PlanProcedureId = 1
        };

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        result.Value.Should().BeEmpty();
        result.Succeeded.Should().BeTrue();
    }

    [TestMethod]
    public async Task GetPlanProcedureUsersQuery_UsersFound_ReturnsUsersList()
    {
        // Arrange
        var context = DbContextHelper.CreateContext();
        var sut = new GetPlanProcedureUsersQueryHandler(context);

        var planProcedureId = 1;

        var user1 = new User { UserId = 1, Name = "Aakash" };
        var user2 = new User { UserId = 2, Name = "Rohan" };

        context.Users.Add(user1);
        context.Users.Add(user2);

        context.PlanProcedureUsers.Add(new PlanProcedureUser
        {
            PlanProcedureId = planProcedureId,
            UserId = user1.UserId,
            User = user1
        });

        context.PlanProcedureUsers.Add(new PlanProcedureUser
        {
            PlanProcedureId = planProcedureId,
            UserId = user2.UserId,
            User = user2
        });

        await context.SaveChangesAsync();

        var request = new GetPlanProcedureUsersQuery
        {
            PlanProcedureId = planProcedureId
        };

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(2);
        result.Value.Should().ContainSingle(u => u.UserId == 1 && u.Name == "Aakash");
        result.Value.Should().ContainSingle(u => u.UserId == 2 && u.Name == "Rohan");
        result.Succeeded.Should().BeTrue();
    }
}
