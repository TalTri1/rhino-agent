﻿/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Gravity.Services.DataContracts;

using Microsoft.AspNetCore.Mvc;

using Rhino.Api.Contracts;
using Rhino.Api.Contracts.AutomationProvider;
using Rhino.Api.Contracts.Configuration;
using Rhino.Controllers.Domain.Interfaces;
using Rhino.Controllers.Extensions;
using Rhino.Controllers.Models;
using Rhino.Controllers.Models.Server;

using Swashbuckle.AspNetCore.Annotations;

using System.Net.Mime;

namespace Rhino.Controllers.Controllers
{
    [ApiVersion("3.0")]
    [Route("api/v{version:apiVersion}/rhino/async")]
    [ApiController]
    public class RhinoAsyncController : ControllerBase
    {
        // members: state
        private readonly IDomain _domain;

        // members: private properties
        private Authentication Authentication => Request.GetAuthentication();

        /// <summary>
        /// Creates a new instance of <see cref="ControllerBase"/>.
        /// </summary>
        /// <param name="domain">An IDomain implementation to use with the Controller.</param>
        public RhinoAsyncController(IDomain domain)
        {
            _domain = domain;
        }

        #region *** Configurations ***
        // POST api/v3/rhino/configurations/invoke
        [HttpPost, Route("configurations/invoke")]
        [SwaggerOperation(
            Summary = "Start-Configuration",
            Description = "Invokes a single _**Rhino Configuration**_ without saving the configuration under Rhino Server State.")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status201Created, SwaggerDocument.StatusCode.Status201Created, Type = typeof(AsyncInvokeModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<RhinoConfiguration>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<RhinoConfiguration>))]
        public IActionResult StartConfiguration([FromBody, SwaggerRequestBody(SwaggerDocument.Parameter.Entity)] RhinoConfiguration configuration)
        {
            // invoke
            var invokeResponse = _domain.RhinoAsync.SetAuthentication(Authentication).StartConfiguration(configuration);

            // get
            return Created($"/api/v3/rhino/async/status/{invokeResponse.Id}", invokeResponse);
        }

        // GET api/v3/rhino/configurations/invoke/:id
        [HttpGet, Route("configurations/invoke/{id}")]
        [SwaggerOperation(
            Summary = "Start-Configuration -Id 00000000-0000-0000-0000-000000000000",
            Description = "Invokes a single _**Rhino Configuration**_ without saving the configuration under Rhino Server State.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status201Created, SwaggerDocument.StatusCode.Status201Created, Type = typeof(AsyncInvokeModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult StartConfiguration([FromRoute, SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // invoke
            var invokeResponse = _domain.RhinoAsync.SetAuthentication(Authentication).StartConfiguration(id);

            // get
            return Created($"/api/v3/rhino/async/status/{invokeResponse.Id}", invokeResponse);
        }
        #endregion

        #region *** Collections    ***
        // POST api/v3/rhino/async/configurations/:id/collections/invoke
        [HttpPost, Route("configurations/{id}/collections/invoke")]
        [SwaggerOperation(
            Summary = "Start-Collection -Configuration {00000000-0000-0000-0000-000000000000}",
            Description = "Invokes _**Rhino Spec**_ directly from the request body using preexisting configuration.")]
        [Consumes(MediaTypeNames.Text.Plain)]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status201Created, SwaggerDocument.StatusCode.Status201Created, Type = typeof(AsyncInvokeModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> StartCollection([SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // setup
            var collection = (await Request.ReadAsync().ConfigureAwait(false))
                .Split(RhinoSpecification.Separator)
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrEmpty(i));
            var configuration = _domain.Configurations.SetAuthentication(Authentication).Get(id);

            // not found
            if (configuration.StatusCode == StatusCodes.Status404NotFound)
            {
                var notFound = $"Invoke-Collection -Configuration {id} = (NotFound, NoConfiguration)";
                return await this
                    .ErrorResultAsync<string>(notFound, configuration.StatusCode)
                    .ConfigureAwait(false);
            }

            // invoke
            configuration.Entity.TestsRepository = collection;
            var invokeResponse = _domain.RhinoAsync.SetAuthentication(Authentication).StartConfiguration(configuration.Entity);

            // get
            return Created($"/api/v3/rhino/async/status/{invokeResponse.Id}", invokeResponse);
        }

        // GET api/v3/rhino/async/configurations/:configuration/collections/:collection/invoke
        [HttpGet, Route("configurations/{configuration}/collections/invoke/{collection}")]
        [SwaggerOperation(
            Summary = "Start-Collection -Configuration 00000000-0000-0000-0000-000000000000 -Collection 00000000-0000-0000-0000-000000000000",
            Description = "Invokes _**Rhino Spec**_ from the application state using preexisting configuration.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status201Created, SwaggerDocument.StatusCode.Status201Created, Type = typeof(AsyncInvokeModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> StartCollection(
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string configuration,
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string collection)
        {
            // constants
            var error = "Start-Collection " +
                    $"-Configuration {configuration} " +
                    $"-Collection {collection} = ($(error), NoCollection | NoConfiguration)";

            // bad request
            var isConfiguration = !string.IsNullOrEmpty(configuration);
            var isCollection = !string.IsNullOrEmpty(collection);

            if (!isConfiguration || !isCollection)
            {
                return await this
                    .ErrorResultAsync<string>(error.Replace("$(error)", "BadRequest"), StatusCodes.Status400BadRequest)
                    .ConfigureAwait(false);
            }

            // setup
            var (collectionStatusCode, collectionEntity) = _domain.Tests.SetAuthentication(Authentication).Get(id: collection);
            var (configurationStatusCode, configurationEntity) = _domain.Configurations.SetAuthentication(Authentication).Get(id: configuration);

            // not found
            isConfiguration = configurationStatusCode == StatusCodes.Status200OK;
            isCollection = collectionStatusCode == StatusCodes.Status200OK;

            if (!isConfiguration || !isCollection)
            {
                return await this
                    .ErrorResultAsync<string>(error.Replace("$(error)", "NotFound"), StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // invoke
            configurationEntity.TestsRepository = collectionEntity.RhinoTestCaseModels.Select(i => i.RhinoSpec);
            var invokeResponse = _domain.RhinoAsync.SetAuthentication(Authentication).StartConfiguration(configurationEntity);

            // get
            return Created($"/api/v3/rhino/async/status/{invokeResponse.Id}", invokeResponse);
        }

        // GET api/v3/rhino/async/collections/invoke/:id
        [HttpGet, Route("collections/invoke/{id}")]
        [SwaggerOperation(
            Summary = "Start-Collection -Configuration All -Collection {00000000-0000-0000-0000-000000000000} -Parallel {True|False}",
            Description = "Invokes _**Rhino Spec**_ from the application state using preexisting collection.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<RhinoTestRun>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, SwaggerDocument.StatusCode.Status400BadRequest, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> InvokeCollection(
            [SwaggerParameter(SwaggerDocument.Parameter.Id)] string id,
            [FromQuery(Name = "parallel"), SwaggerParameter(SwaggerDocument.Parameter.Parallel, Required = false)] bool isParallel,
            [FromQuery(Name = "maxParallel"), SwaggerParameter(SwaggerDocument.Parameter.MaxParallel, Required = false)] int maxParallel = 0)
        {
            // constants
            var error = "Start-Collection" +
                " -Configuration All " +
                $"-Collection {id} " +
                $"-Parallel {isParallel} = ($(error), NoCollection | NoConfiguration)";

            // bad request
            if (string.IsNullOrEmpty(id))
            {
                return await this
                    .ErrorResultAsync<string>(error.Replace("$(error)", "BadRequest"), StatusCodes.Status400BadRequest)
                    .ConfigureAwait(false);
            }

            // setup
            var (statusCode, collectionEntity) = _domain.Tests.SetAuthentication(Authentication).Get(id);

            // not found
            if (statusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>(error.Replace("$(error)", "NotFound"), StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // setup
            var configurations = collectionEntity
                .Configurations
                .Select(i => _domain.Configurations.SetAuthentication(Authentication).Get(id: i))
                .Where(i => i.StatusCode == StatusCodes.Status200OK)
                .Select(i => i.Entity)
                .ToArray();

            // not found
            var isConfiguration = configurations.Length > 0;
            var isCollection = statusCode == StatusCodes.Status200OK;

            if (!isConfiguration || !isCollection)
            {
                return await this
                    .ErrorResultAsync<string>(error.Replace("$(error)", "NotFound"), StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // build
            maxParallel = isParallel ? maxParallel : 1;
            maxParallel = maxParallel <= 0 ? Environment.ProcessorCount : maxParallel;

            // invoke
            var invokeResponse = _domain
                .RhinoAsync
                .SetAuthentication(Authentication)
                .StartCollection($"{collectionEntity.Id}", isParallel, maxParallel);

            // get
            var queryParams = invokeResponse.Select(i => $"id={i.Id}");
            var query = "?" + string.Join("&", queryParams);
            return Created($"/api/v3/rhino/async/status{query}", invokeResponse);
        }
        #endregion

        #region *** Get            ***
        // GET api/v3/rhino/async/status/:id
        [HttpGet("status/{id}")]
        [SwaggerOperation(
            Summary = "Get-InvokeStatus -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Gets the status of the provided asynchronous _**Invoke**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(AsyncStatusModel<RhinoConfiguration>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetStatus([SwaggerParameter(SwaggerDocument.Parameter.Id)] Guid id)
        {
            // invoke
            var response = _domain.RhinoAsync.SetAuthentication(Authentication).GetStatus(id);

            // not found
            if (response.StatusCode == StatusCodes.Status404NotFound)
            {
                return await this
                    .ErrorResultAsync<string>($"Get-InvokeStatus -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // get
            return Ok(response.Status);
        }

        // GET api/v3/rhino/async/status?id=
        [HttpGet("status")]
        [SwaggerOperation(
            Summary = "Get-InvokeStatus -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Gets the status of the provided asynchronous _**Invoke**_.")]
        [Produces(MediaTypeNames.Application.Json)]
        [SwaggerResponse(StatusCodes.Status200OK, SwaggerDocument.StatusCode.Status200OK, Type = typeof(IEnumerable<AsyncStatusModel<RhinoConfiguration>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> GetStatus([SwaggerParameter(SwaggerDocument.Parameter.Id), FromQuery] IEnumerable<Guid> id)
        {
            // setup
            var response = _domain.RhinoAsync.SetAuthentication(Authentication).GetStatus();

            // all
            if (!id.Any())
            {
                Response.Headers.Add(RhinoResponseHeader.CountTotalInvokes, $"{response.Count()}");
                return Ok(response);
            }

            // setup
            var ids = id.Select(i => $"{i}".ToUpper()).ToArray();

            // invoke
            response = response.Where(i => ids.Contains($"{i.RuntimeId}".ToUpper()));

            // not found
            if (!response.Any())
            {
                return await this
                    .ErrorResultAsync<string>($"Get-InvokeStatus -Id {id} = NotFound", StatusCodes.Status404NotFound)
                    .ConfigureAwait(false);
            }

            // add count header
            Response.Headers.Add(RhinoResponseHeader.CountTotalInvokes, $"{response.Count()}");

            // get
            return Ok(response.Select(i => i));
        }
        #endregion

        #region *** Delete         ***
        // DELETE: api/v3/rhino/async/status/:id
        [HttpDelete("status/{id}")]
        [SwaggerOperation(
            Summary = "Delete-InvokeStatus -Id {00000000-0000-0000-0000-000000000000}",
            Description = "Deletes an existing _**Invoke**_.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, SwaggerDocument.StatusCode.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status404NotFound, SwaggerDocument.StatusCode.Status404NotFound, Type = typeof(GenericErrorModel<string>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public async Task<IActionResult> Delete([FromRoute, SwaggerParameter(SwaggerDocument.Parameter.Id)] string id)
        {
            // get credentials
            var statusCode = _domain.RhinoAsync.SetAuthentication(Authentication).Delete(id);

            // results
            return statusCode == StatusCodes.Status404NotFound
                ? await this.ErrorResultAsync<string>($"Delete-InvokeStatus -Id {id} = NotFound", statusCode).ConfigureAwait(false)
                : NoContent();
        }

        // DELETE: api/v3/rhino/async/status
        [HttpDelete("status")]
        [SwaggerOperation(
            Summary = "Delete-InvokeStatus -All",
            Description = "Deletes all existing _**Invokes**_ for the authenticated user.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, SwaggerDocument.StatusCode.Status204NoContent)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, SwaggerDocument.StatusCode.Status500InternalServerError, Type = typeof(GenericErrorModel<string>))]
        public IActionResult Delete()
        {
            // get credentials
            _domain.RhinoAsync.SetAuthentication(Authentication).Delete();

            // results
            return NoContent();
        }
        #endregion
    }
}
