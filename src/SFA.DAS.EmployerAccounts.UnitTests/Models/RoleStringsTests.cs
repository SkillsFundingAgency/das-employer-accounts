using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Models;

namespace SFA.DAS.EmployerAccounts.UnitTests.Models;

public class RoleStringsTests
{
    [TestCase(Role.None, RoleStrings.NoneDescription)]
    [TestCase(Role.Owner, RoleStrings.OwnerDescription)]
    [TestCase(Role.Viewer, RoleStrings.ViewerDescription)]
    [TestCase(Role.Transactor, RoleStrings.TransactorDescription)]
    public void GetRoleDescription_ReturnsRespectiveRoleDescription(Role role, string expected)
    {
        var actual = RoleStrings.GetRoleDescription(role);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetRoleDescription_InvalidRole_ThrowsArgumentException()
    {
        var action = () => RoleStrings.GetRoleDescription("invalid");
        action.Should().Throw<ArgumentException>();
    }
}
