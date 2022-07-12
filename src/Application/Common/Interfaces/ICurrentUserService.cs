﻿namespace Hippo.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    bool IsAdministrator { get; }
}
