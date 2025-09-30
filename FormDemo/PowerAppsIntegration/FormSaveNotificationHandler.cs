using Umbraco.Forms.Core.Services.Notifications;
using Umbraco.Cms.Core.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Xrm.Sdk.Metadata;
using FormDemo.PowerAppsIntegration;
using Microsoft.Xrm.Sdk;

public class FormSaveNotificationHandler : INotificationHandler<FormSavedNotification>
{
    private readonly ILogger<FormSaveNotificationHandler> _logger;
    private readonly IConfiguration _configuration;

    public FormSaveNotificationHandler(
        ILogger<FormSaveNotificationHandler> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public void Handle(FormSavedNotification notification)
    {
        try
        {
            if (notification?.SavedEntities == null)
            {
                _logger.LogWarning("FormSavedNotification: Notification or SavedEntities is null");
                return;
            }

            var form = notification.SavedEntities.FirstOrDefault();
            if (form == null)
            {
                _logger.LogWarning("FormSavedNotification: Form is null");
                return;
            }

            _logger.LogInformation($"Form saved: {form.Name} with {form.AllFields?.Count() ?? 0} fields");

            // Create table asynchronously
            _ = Task.Run(() => CreateOrUpdatePowerAppsTableAsync(form, form.Id.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling form saved notification");
        }
    }
    private async Task CreateOrUpdatePowerAppsTableAsync(Umbraco.Forms.Core.Models.Form form, string formId)
    {
        try
        {
            var tableName = $"new_umbracoform_{formId.Replace("-", "_").ToLower()}";

            if (form?.AllFields == null)
            {
                _logger.LogWarning("Form or AllFields is null");
                return;
            }

            _logger.LogInformation($"Creating Power Apps table: {tableName} with {form.AllFields.Count()} fields");

            var dynamicColumns = new List<AttributeMetadata>();

            foreach (var field in form.AllFields)
            {
                var columnName = GetSafeColumnName(field);
                var dataType = MapFieldTypeToDataverseType(field);

                _logger.LogInformation($"Processing field: '{field.Caption}' -> Column: '{columnName}' -> Type: '{dataType.Type}' (FieldTypeId: {field.FieldTypeId})");

                AttributeMetadata columnMetadata = dataType.Type switch
                {
                    "String" => new StringAttributeMetadata
                    {
                        SchemaName = columnName,
                        DisplayName = new Label(field.Caption ?? columnName, 1033),
                        RequiredLevel = new AttributeRequiredLevelManagedProperty(
                            field.Mandatory ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                        MaxLength = dataType.MaxLength ?? 255,
                        FormatName = StringFormatName.Text
                    },
                    "Integer" => new IntegerAttributeMetadata
                    {
                        SchemaName = columnName,
                        DisplayName = new Label(field.Caption ?? columnName, 1033),
                        RequiredLevel = new AttributeRequiredLevelManagedProperty(
                            field.Mandatory ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None)
                    },
                    "DateTime" => new DateTimeAttributeMetadata
                    {
                        SchemaName = columnName,
                        DisplayName = new Label(field.Caption ?? columnName, 1033),
                        RequiredLevel = new AttributeRequiredLevelManagedProperty(
                            field.Mandatory ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                        Format = DateTimeFormat.DateAndTime
                    },
                    "Boolean" => new BooleanAttributeMetadata
                    {
                        SchemaName = columnName,
                        DisplayName = new Label(field.Caption ?? columnName, 1033),
                        RequiredLevel = new AttributeRequiredLevelManagedProperty(
         field.Mandatory ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                        OptionSet = new BooleanOptionSetMetadata(
         new OptionMetadata(new Label("Yes", 1033), 1),
         new OptionMetadata(new Label("No", 1033), 0)
     ),
                        DefaultValue = false
                    },
                    "Memo" => new MemoAttributeMetadata
                    {
                        SchemaName = columnName,
                        DisplayName = new Label(field.Caption ?? columnName, 1033),
                        RequiredLevel = new AttributeRequiredLevelManagedProperty(
                            field.Mandatory ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                        MaxLength = dataType.MaxLength ?? 2000
                    },
                    _ => new StringAttributeMetadata
                    {
                        SchemaName = columnName,
                        DisplayName = new Label(field.Caption ?? columnName, 1033),
                        RequiredLevel = new AttributeRequiredLevelManagedProperty(
                            field.Mandatory ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None),
                        MaxLength = 255,
                        FormatName = StringFormatName.Text
                    }
                };

                dynamicColumns.Add(columnMetadata);
                _logger.LogInformation($"✅ Added column metadata: {columnName} ({dataType.Type}) - {field.Caption} [FieldTypeId: {field.FieldTypeId}]");
            }

            _logger.LogInformation($"📋 Total columns prepared: {dynamicColumns.Count}");

            var dataverseService = new DataverseWebApiService(_configuration);
            await dataverseService.CreateOrUpdateDynamicTableAsync(tableName, form.Name, dynamicColumns);

            _logger.LogInformation($"Successfully created table '{tableName}' with {dynamicColumns.Count} columns");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create Power Apps table for form {form.Name}");
        }
    }

    private string GetSafeColumnName(Umbraco.Forms.Core.Models.Field field)
    {
        try
        {
            // Use alias if available, otherwise use caption, otherwise use field ID
            string baseName = !string.IsNullOrEmpty(field.Alias)
                ? field.Alias
                : !string.IsNullOrEmpty(field.Caption)
                    ? field.Caption
                    : field.Id.ToString();

            // Clean the name and ensure it starts with a letter and contains only valid characters
            var safeName = System.Text.RegularExpressions.Regex.Replace(baseName, @"[^a-zA-Z0-9_]", "_").ToLower();

            // Ensure it starts with a letter (Dataverse requirement)
            if (!char.IsLetter(safeName[0]))
            {
                safeName = "field_" + safeName;
            }

            // Add prefix to avoid conflicts
            return $"new_{safeName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating safe column name");
            return $"new_field_{field.Id.ToString("N")[..8]}";
        }
    }
    private (string Type, int? MaxLength) MapFieldTypeToDataverseType(Umbraco.Forms.Core.Models.Field field)
    {
        try
        {
            var fieldTypeId = field.FieldTypeId.ToString().ToLower();
            _logger.LogInformation($"Processing field '{field.Caption}' with FieldTypeId: {fieldTypeId}");

            // Map based on FieldTypeId
            return fieldTypeId switch
            {
                "3f92e01b-29e2-4a30-bf33-9df5580ed52c" => ("String", 255),   // Short Answer
                "023f09ac-1445-4bcb-b8fa-ab49f33bd046" => ("Memo", 2000),   // Long Answer → Multiple lines text
                "f8b4c3b8-af28-11de-9dd8-ef5956d89593" => ("DateTime", null), // Date
                "903df9b0-a78c-11de-9fc1-db7a56d89593" => ("String", 255),   // Single Choice
                "84a17cf8-b711-46a6-9840-0e4a072ad000" => ("String", 500),   // File Upload
                "d5c0c390-ae9a-11de-a69e-666455d89593" => ("Boolean", null), // Checkbox
                "fab43f20-a6bf-11de-a28f-9b5755d89593" => ("String", 255),   // Multiple Choice
                "0dd29d42-a6a5-11de-a2f2-222256d89593" => ("String", 255),   // Dropdown
                "fb37bc60-d41e-11de-aeae-37c155d89593" => ("String", 255),   // Password
                "e3fbf6c4-f46c-495e-aff8-4b3c227b4a98" => ("Memo", 2000),    // Title and Description
                "1f8d45f8-76e6-4550-a0f5-9637b8454619" => ("Memo", 2000),    // Richtext
                "da206cae-1c52-434e-b21a-4a7c198af877" => ("String", 255),   // Hidden
                "b69deaeb-ed75-4dc9-bfb8-d036bf9d3730" => ("String", 255),   // RecaptchaV2
                "663aa19b-423d-4f38-a1d6-c840c926ef86" => ("String", 255),   // RecaptchaV3
                _ => ("String", 255) // Default
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping field type to Dataverse type");
            return ("String", 255);
        }
    }


}