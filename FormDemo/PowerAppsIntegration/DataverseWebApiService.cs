using FormDemo.PowerAppsIntegration.Interface;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace FormDemo.PowerAppsIntegration
{
    public class DataverseWebApiService : IDataverseWebApiService
    {
        private readonly IConfiguration _configuration;
        private readonly string clientId;
        private readonly string tenantId;
        private readonly string clientSecret;
        private readonly string url;

        public DataverseWebApiService(IConfiguration configuration)
        {
            _configuration = configuration;
             clientId = _configuration["Dataverse:ClientId"];
             tenantId = _configuration["Dataverse:TenantId"];
             clientSecret = _configuration["Dataverse:ClientSecret"];
             url = _configuration["Dataverse:Url"];
        }

        public async Task CreateOrUpdateDynamicTableAsync(string tableName, string displayName, List<AttributeMetadata> dynamicColumns)
        {
            try
            {
                var connectionString = $@"
                    AuthType=ClientSecret;
                    Url={url};
                    ClientId={clientId};
                    ClientSecret={clientSecret};
                    TenantId={tenantId};
                ";

                using var serviceClient = new ServiceClient(connectionString);

                if (!serviceClient.IsReady)
                {
                    throw new Exception($"Failed to connect to Dataverse: {serviceClient.LastError}");
                }

                // Check if table already exists
                bool tableExists = await CheckIfTableExistsAsync(serviceClient, tableName);

                if (!tableExists)
                {
                    // Create new table
                    await CreateNewTableAsync(serviceClient, tableName, displayName);
                }

                // Get existing columns to avoid duplicates
                var existingColumns = await GetExistingColumnsAsync(serviceClient, tableName);

                // Add new columns that don't exist
                var successCount = 0;
                var failCount = 0;

                foreach (var column in dynamicColumns)
                {
                    if (!existingColumns.Contains(column.SchemaName.ToLower()))
                    {
                        try
                        {
                            var createAttributeRequest = new CreateAttributeRequest
                            {
                                EntityName = tableName,
                                Attribute = column
                            };

                            await serviceClient.ExecuteAsync(createAttributeRequest);
                            successCount++;

                            // Log successful creation
                            Console.WriteLine($"✅ Successfully created column '{column.SchemaName}' ({column.GetType().Name})");
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            Console.WriteLine($"❌ Failed to create column '{column.SchemaName}': {ex.Message}");

                            // Log detailed error information
                            if (ex.InnerException != null)
                            {
                                Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
                            }

                            // Don't throw here, continue with other columns
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⏭️ Column '{column.SchemaName}' already exists, skipping");
                    }
                }

                Console.WriteLine($"📊 Column creation summary: {successCount} successful, {failCount} failed, {existingColumns.Count} already existed");

                if (failCount > 0)
                {
                    throw new Exception($"Failed to create {failCount} out of {dynamicColumns.Count} columns. Check logs for details.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating or updating dynamic table: {ex.Message}", ex);
            }
        }

        private async Task<bool> CheckIfTableExistsAsync(ServiceClient serviceClient, string tableName)
        {
            try
            {
                var request = new RetrieveEntityRequest
                {
                    LogicalName = tableName,
                    EntityFilters = EntityFilters.Entity
                };

                await serviceClient.ExecuteAsync(request);
                return true; // Table exists
            }
            catch (Exception)
            {
                return false; // Table doesn't exist
            }
        }

        private async Task CreateNewTableAsync(ServiceClient serviceClient, string tableName, string displayName)
        {
            var tableDef = new CreateEntityRequest
            {
                Entity = new EntityMetadata
                {
                    SchemaName = tableName,
                    DisplayName = new Label(displayName ?? tableName, 1033),
                    DisplayCollectionName = new Label($"{displayName ?? tableName} Records", 1033),
                    Description = new Label($"Dynamically created table for Umbraco form: {displayName}", 1033),
                    OwnershipType = OwnershipTypes.UserOwned,
                    IsActivity = false
                },
                PrimaryAttribute = new StringAttributeMetadata
                {
                    SchemaName = "new_recordname", // Changed to a more generic name
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.ApplicationRequired),
                    MaxLength = 100,
                    FormatName = StringFormatName.Text,
                    DisplayName = new Label("Record Name", 1033),
                    Description = new Label("Primary identifier for the record", 1033)
                }
            };

            await serviceClient.ExecuteAsync(tableDef);
        }

        private async Task<HashSet<string>> GetExistingColumnsAsync(ServiceClient serviceClient, string tableName)
        {
            try
            {
                var request = new RetrieveEntityRequest
                {
                    LogicalName = tableName,
                    EntityFilters = EntityFilters.Attributes
                };

                var response = (RetrieveEntityResponse)await serviceClient.ExecuteAsync(request);

                return new HashSet<string>(
                    response.EntityMetadata.Attributes
                        .Select(attr => attr.SchemaName.ToLower()),
                    StringComparer.OrdinalIgnoreCase
                );
            }
            catch (Exception)
            {
                return new HashSet<string>();
            }
        }

        ///
        public async Task<Guid> CreateRowAsync(string tableLogicalName, IDictionary<string, object> attributes)
        {
            var connectionString = $@"
                    AuthType=ClientSecret;
                    Url={url};
                    ClientId={clientId};
                    ClientSecret={clientSecret};
                    TenantId={tenantId};
                ";

            using var serviceClient = new ServiceClient(connectionString);

            var entity = new Entity(tableLogicalName);
            foreach (var kv in attributes)
            {
                // DBNull.Value is not required; simply omit nulls if undesired
                if (kv.Value != null) entity[kv.Key] = kv.Value;
            }

            // Create via IOrganizationService
            return serviceClient.Create(entity);
            // Or: var resp = (CreateResponse)await serviceClient.ExecuteAsync(new CreateRequest { Target = entity });
            // return resp.id;
        }


    }
}