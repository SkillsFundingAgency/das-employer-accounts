﻿namespace SFA.DAS.EmployerAccounts.Web;

public class OrchestratorResponse
{
    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
    public Exception Exception { get; set; }
    public FlashMessageViewModel FlashMessage { get; set; }
    public string RedirectUrl { get; set; }
    public string RedirectButtonMessage { get; set; }
}

public class OrchestratorResponse<T> : OrchestratorResponse
{
    public T Data { get; set; }
}