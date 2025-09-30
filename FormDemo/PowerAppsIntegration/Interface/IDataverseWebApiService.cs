using Microsoft.Xrm.Sdk.Metadata;

namespace FormDemo.PowerAppsIntegration.Interface
{
    public interface IDataverseWebApiService
    {
        Task CreateOrUpdateDynamicTableAsync(string tableName, string displayName, List<AttributeMetadata> dynamicColumns);
        Task<Guid> CreateRowAsync(string tableLogicalName, IDictionary<string, object> attributes);
    }
}
