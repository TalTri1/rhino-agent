﻿using Microsoft.Extensions.Configuration;

using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Models;

namespace Rhino.Controllers.Domain.Interfaces
{
    public interface IDomain
    {
        IApplicationRepository Application { get; set; }
        IConfiguration AppSettings { get; set; }
        IRepository<RhinoConfiguration> Configurations { get; set; }
        IEnvironmentRepository Environments { get; set; }
        ILogsRepository Logs { get; set; }
        IMetaDataRepository MetaData { get; set; }
        IRepository<RhinoModelCollection> Models { get; set; }
        IPluginsRepository Plugins { get; set; }
        IRhinoRepository Rhino { get; set; }
        IRhinoAsyncRepository RhinoAsync { get; set; }
        ITestsRepository Tests { get; set; }
    }
}
