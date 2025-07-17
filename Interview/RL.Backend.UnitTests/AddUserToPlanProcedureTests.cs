using FluentAssertions;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using RL.Backend.Commands;
using RL.Backend.Commands.Handlers.PlanProcedure;
using RL.Backend.Exceptions;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.UnitTests;

[TestClass]
public class AddUserToPlanProcedureTests
{
    private Mock<ILogger<AddUserToPlanProcedureCommandHandler>> _mockLogger = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockLogger = new Mock<ILogger<AddUserToPlanProcedureCommandHandler>>();
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(int.MinValue)]
    public async Task AddUserToPlanProcedureTests_InvalidPlanProcedureId_ReturnsBadRequest(int planProcedureId)
    {
        // Given
        var context = new Mock<RLContext>();
        var sut = new AddUserToPlanProcedureCommandHandler(context.Object, _mockLogger.Object);
        var request = new AddUserToPlanProcedureCommand()
        {
            PlanProcedureId = planProcedureId,
            UserId = 1
        };

        // When
        var result = await sut.Handle(request, new CancellationToken());

        // Then
        result.Exception.Should().BeOfType(typeof(BadRequestException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(int.MinValue)]
    public async Task AddUserToPlanProcedureTests_InvalidUserId_ReturnsBadRequest(int userId)
    {
        // Given
        var context = new Mock<RLContext>();
        var sut = new AddUserToPlanProcedureCommandHandler(context.Object, _mockLogger.Object);
        var request = new AddUserToPlanProcedureCommand()
        {
            PlanProcedureId = 1,
            UserId = userId
        };

        // When
        var result = await sut.Handle(request, new CancellationToken());

        // Then
        result.Exception.Should().BeOfType(typeof(BadRequestException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(20)]
    public async Task AddUserToPlanProcedureTests_PlanProcedureNotFound_ReturnsNotFound(int planProcedureId)
    {
        // Given
        var context = DbContextHelper.CreateContext();
        var sut = new AddUserToPlanProcedureCommandHandler(context, _mockLogger.Object);
        var request = new AddUserToPlanProcedureCommand()
        {
            PlanProcedureId = planProcedureId,
            UserId = 1
        };

        // Add different planProcedure to ensure ID won't match
        context.PlanProcedures.Add(new PlanProcedure
        {
            PlanProcedureId = planProcedureId + 1
        });
        context.Users.Add(new User
        {
            UserId = 1,
            Name = "Test User"
        });
        await context.SaveChangesAsync();

        // When
        var result = await sut.Handle(request, new CancellationToken());

        // Then
        result.Exception.Should().BeOfType(typeof(NotFoundException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(20)]
    public async Task AddUserToPlanProcedureTests_UserNotFound_ReturnsNotFound(int userId)
    {
        // Given
        var context = DbContextHelper.CreateContext();
        var sut = new AddUserToPlanProcedureCommandHandler(context, _mockLogger.Object);
        var request = new AddUserToPlanProcedureCommand()
        {
            PlanProcedureId = 1,
            UserId = userId
        };

        context.PlanProcedures.Add(new PlanProcedure
        {
            PlanProcedureId = 1
        });
        context.Users.Add(new User
        {
            UserId = userId + 1,
            Name = "Other User"
        });
        await context.SaveChangesAsync();

        // When
        var result = await sut.Handle(request, new CancellationToken());

        // Then
        result.Exception.Should().BeOfType(typeof(NotFoundException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(42, 99)]
    public async Task AddUserToPlanProcedureTests_AlreadyHasUser_ReturnsSuccess(int planProcedureId, int userId)
    {
        // Given
        var context = DbContextHelper.CreateContext();
        var sut = new AddUserToPlanProcedureCommandHandler(context, _mockLogger.Object);
        var request = new AddUserToPlanProcedureCommand()
        {
            PlanProcedureId = planProcedureId,
            UserId = userId
        };

        context.PlanProcedures.Add(new PlanProcedure
        {
            PlanProcedureId = planProcedureId,
            PlanProcedureUsers = new List<PlanProcedureUser>
            {
                new PlanProcedureUser { UserId = userId }
            }
        });
        context.Users.Add(new User
        {
            UserId = userId,
            Name = "Already Assigned"
        });
        await context.SaveChangesAsync();

        // When
        var result = await sut.Handle(request, new CancellationToken());

        // Then
        result.Value.Should().BeOfType(typeof(Unit));
        result.Succeeded.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(42, 99)]
    public async Task AddUserToPlanProcedureTests_AddNewUser_ReturnsSuccess(int planProcedureId, int userId)
    {
        // Given
        var context = DbContextHelper.CreateContext();
        var sut = new AddUserToPlanProcedureCommandHandler(context, _mockLogger.Object);
        var request = new AddUserToPlanProcedureCommand()
        {
            PlanProcedureId = planProcedureId,
            UserId = userId
        };

        context.PlanProcedures.Add(new PlanProcedure
        {
            PlanProcedureId = planProcedureId
        });
        context.Users.Add(new User
        {
            UserId = userId,
            Name = "New User"
        });
        await context.SaveChangesAsync();

        // When
        var result = await sut.Handle(request, new CancellationToken());

        // Then
        var dbEntry = await context.PlanProcedureUsers
            .FirstOrDefaultAsync(ppu => ppu.PlanProcedureId == planProcedureId && ppu.UserId == userId);

        dbEntry.Should().NotBeNull();
        result.Value.Should().BeOfType(typeof(Unit));
        result.Succeeded.Should().BeTrue();
    }
}
