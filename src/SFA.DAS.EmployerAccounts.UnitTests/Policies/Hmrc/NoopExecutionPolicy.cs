using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerAccounts.Policies.Hmrc;

namespace SFA.DAS.EmployerAccounts.UnitTests.Policies.Hmrc;

public class NoopExecutionPolicy : ExecutionPolicy
{
    public override Task ExecuteAsync(Func<Task> action) => action();

    public override Task<T> ExecuteAsync<T>(Func<Task<T>> func) => func();
}