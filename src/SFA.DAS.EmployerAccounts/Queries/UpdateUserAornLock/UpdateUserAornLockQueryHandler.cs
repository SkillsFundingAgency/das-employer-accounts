﻿using System.Threading;

namespace SFA.DAS.EmployerAccounts.Queries.UpdateUserAornLock;

public class UpdateUserAornLockQueryHandler : IRequestHandler<UpdateUserAornLockRequest>
{
    private readonly IUserAornPayeLockService _userAornPayeLockService;

    public UpdateUserAornLockQueryHandler(IUserAornPayeLockService userAornPayeLockService)
    {
        _userAornPayeLockService = userAornPayeLockService;
    }

    public async Task Handle(UpdateUserAornLockRequest request, CancellationToken cancellationToken)
    {
        await _userAornPayeLockService.UpdateUserAornPayeAttempt(request.UserRef, request.Success);
    }
}