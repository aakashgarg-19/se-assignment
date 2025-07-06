using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Moq;

using RL.Backend.Commands;
using RL.Backend.Commands.Handlers.PlanProcedure;
using RL.Backend.Exceptions;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.UnitTests;

[TestClass]
public class RemoveUserFromPlanProcedureTests
{
    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    public async Task RemoveUserFromPlanProcedure_InvalidPlanProcedureId_ReturnsBadRequest(int planProcedureId)
    {
        // Arrange
        var context = new Mock<RLContext>();
        var sut = new RemoveUserFromPlanProcedureCommandHandler(context.Object);

        var request = new RemoveUserFromPlanProcedureCommand
        {
            PlanProcedureId = planProcedureId,
            UserId = "1"
        };

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        result.Exception.Should().BeOfType<BadRequestException>();
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    [DataRow("   ")]
    public async Task RemoveUserFromPlanProcedure_InvalidUserId_ReturnsBadRequest(string userId)
    {
        // Arrange
        var context = new Mock<RLContext>();
        var sut = new RemoveUserFromPlanProcedureCommandHandler(context.Object);

        var request = new RemoveUserFromPlanProcedureCommand
        {
            PlanProcedureId = 1,
            UserId = userId
        };

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        result.Exception.Should().BeOfType<BadRequestException>();
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow("abc")]
    [DataRow("1.23")]
    [DataRow("invalid")]
    public async Task RemoveUserFromPlanProcedure_NonIntUserId_ReturnsBadRequest(string userId)
    {
        // Arrange
        var context = DbContextHelper.CreateContext();
        var sut = new RemoveUserFromPlanProcedureCommandHandler(context);

        context.PlanProcedureUsers.Add(new PlanProcedureUser
        {
            PlanProcedureId = 1,
            UserId = 1
        });
        await context.SaveChangesAsync();

        var request = new RemoveUserFromPlanProcedureCommand
        {
            PlanProcedureId = 1,
            UserId = userId
        };

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        result.Exception.Should().BeOfType<BadRequestException>();
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    public async Task RemoveUserFromPlanProcedure_AllUsers_NoUsersFound_ReturnsNotFound()
    {
        // Arrange
        var context = DbContextHelper.CreateContext();
        var sut = new RemoveUserFromPlanProcedureCommandHandler(context);

        var request = new RemoveUserFromPlanProcedureCommand
        {
            PlanProcedureId = 1,
            UserId = "*"
        };

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        result.Exception.Should().BeOfType<NotFoundException>();
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    public async Task RemoveUserFromPlanProcedure_AllUsers_RemovesSuccessfully()
    {
        // Arrange
        var context = DbContextHelper.CreateContext();
        var sut = new RemoveUserFromPlanProcedureCommandHandler(context);

        context.PlanProcedureUsers.Add(new PlanProcedureUser
        {
            PlanProcedureId = 1,
            UserId = 1
        });
        context.PlanProcedureUsers.Add(new PlanProcedureUser
        {
            PlanProcedureId = 1,
            UserId = 2
        });
        await context.SaveChangesAsync();

        var request = new RemoveUserFromPlanProcedureCommand
        {
            PlanProcedureId = 1,
            UserId = "*"
        };

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        var remaining = await context.PlanProcedureUsers.Where(pu => pu.PlanProcedureId == 1).ToListAsync();
        remaining.Should().BeEmpty();
        result.Succeeded.Should().BeTrue();
    }

    [TestMethod]
    public async Task RemoveUserFromPlanProcedure_SingleUser_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var context = DbContextHelper.CreateContext();
        var sut = new RemoveUserFromPlanProcedureCommandHandler(context);

        context.PlanProcedureUsers.Add(new PlanProcedureUser
        {
            PlanProcedureId = 1,
            UserId = 1
        });
        await context.SaveChangesAsync();

        var request = new RemoveUserFromPlanProcedureCommand
        {
            PlanProcedureId = 1,
            UserId = "2"
        };

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        result.Exception.Should().BeOfType<NotFoundException>();
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    public async Task RemoveUserFromPlanProcedure_SingleUser_RemovesSuccessfully()
    {
        // Arrange
        var context = DbContextHelper.CreateContext();
        var sut = new RemoveUserFromPlanProcedureCommandHandler(context);

        context.PlanProcedureUsers.Add(new PlanProcedureUser
        {
            PlanProcedureId = 1,
            UserId = 1
        });
        context.PlanProcedureUsers.Add(new PlanProcedureUser
        {
            PlanProcedureId = 1,
            UserId = 2
        });
        await context.SaveChangesAsync();

        var request = new RemoveUserFromPlanProcedureCommand
        {
            PlanProcedureId = 1,
            UserId = "1"
        };

        // Act
        var result = await sut.Handle(request, CancellationToken.None);

        // Assert
        var remaining = await context.PlanProcedureUsers
            .Where(pu => pu.PlanProcedureId == 1 && pu.UserId == 1)
            .ToListAsync();

        remaining.Should().BeEmpty();
        result.Succeeded.Should().BeTrue();
    }
}
