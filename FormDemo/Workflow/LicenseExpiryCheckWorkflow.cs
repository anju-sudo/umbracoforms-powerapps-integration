using System.Text;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core;
using FormDemo.Interface;
using Umbraco.Forms.Core.Persistence.Dtos;
using FormDemo.CustomModel;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace FormDemo.Workflow
{
    public class LicenseExpiryCheckWorkflow : WorkflowType
    {
        private readonly ILogger<LicenseExpiryCheckWorkflow> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LicenseExpiryCheckWorkflow(
            ILogger<LicenseExpiryCheckWorkflow> logger,
            IServiceProvider serviceProvider,
            IUmbracoContextFactory umbracoContextFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _umbracoContextFactory = umbracoContextFactory;
            _httpContextAccessor = httpContextAccessor;

            this.Id = new Guid("b7a3ff56-c4c6-4e9e-b9b7-2c8c70f14d1f");
            this.Name = "License Expiry Checker";
            this.Description = "Checks license expiry date via API.";
            this.Icon = "icon-flash";
        }

        public override Task<WorkflowExecutionStatus> ExecuteAsync(WorkflowExecutionContext context)
        {
            try
            {
                var licenseNodeIdStr = context.Record.GetRecordFieldByAlias("licenseNumber")?.ValuesAsString();
                if (int.TryParse(licenseNodeIdStr, out int nodeId))
                {
                    var expiryDate = GetExpiryDateFromNode(nodeId);
                    if (expiryDate.HasValue && expiryDate.Value.Date < DateTime.Today)
                    {
                        SetSessionError("License is expired. Please renew your license to continue.");
                        _logger.LogWarning("License {LicenseId} expired on {ExpiryDate}", nodeId, expiryDate.Value);
                        return Task.FromResult(WorkflowExecutionStatus.Failed);
                    }
                }
                else
                {
                    SetSessionError("Invalid license number format.");
                    return Task.FromResult(WorkflowExecutionStatus.Failed);
                }

                ClearSessionError();
                return Task.FromResult(WorkflowExecutionStatus.Completed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in license checker form");
                SetSessionError("An error occurred while validating the license.");
                return Task.FromResult(WorkflowExecutionStatus.Failed);
            }
        }

        private void SetSessionError(string message)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Session != null)
            {
                httpContext.Session.SetString("LicenseValidationError", message);
            }
        }

        private void ClearSessionError()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Session != null)
            {
                httpContext.Session.Remove("LicenseValidationError");
            }
        }
        private DateTime? GetExpiryDateFromNode(int nodeId)
        {
            using var cref = _umbracoContextFactory.EnsureUmbracoContext();
            var content = cref.UmbracoContext.Content.GetById(nodeId);

            if (content == null)
                return null;

            // Change "expiryDate" to your actual property alias in Umbraco
            return content.Value<DateTime?>("expiryDate");
        }

        public override List<Exception> ValidateSettings()
        {
            return new List<Exception>();
        }
    }
}
