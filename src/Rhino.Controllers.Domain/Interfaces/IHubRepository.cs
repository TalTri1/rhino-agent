﻿using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Models;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IHubRepository
    {
        (int StatusCode, object Entity) CreateTestRun(RhinoConfiguration configuration);
        (int StatusCode, RunsStatusModel Entity) GetStatus();
        (int StatusCode, RunStatusModel Entity) GetStatus(string id);
        void Reset();
    }
}
