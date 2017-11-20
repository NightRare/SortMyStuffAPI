using System;

namespace SortMyStuffAPI.Utils
{
    public static class ApiStrings
    {
        #region MESSAGES

        public const string ErrorCommonMsg = "A server error occurred.";

        public const string ErrorCommonDetail = "No further detail.";

        public const string ErrorMemorySizeAttributeMessage = "The size of the {0} should between {1} bytes and {2} bytes.";

        #endregion

        #region HTTP METHOD NAMES

        public const string HttpGet = "GET";

        public const string HttpPost = "POST";

        public const string HttpPut = "PUT";

        public const string HttpDelete = "DELETE";

        public const string HttpPatch = "PATCH";

        #endregion

        #region PARAMETER OPERATORS

        public const string ParameterDesc = "desc";

        public const string ParameterOpEqual = "eq";

        public const string ParameterOpGreaterThan = "gt";

        public const string ParameterOpLessThan = "lt";

        public const string ParameterOpGreaterThanOrEqual = "gte";

        public const string ParameterOpLessThanOrEqual = "lte";

        #endregion

        #region FORM RELATION TYPES

        public const string FormRel = "form";

        public const string FormEditRel = "edit-form";

        public const string FormCreateRel = "create-form";

        public const string FormQueryRel = "query-form";

        #endregion

        #region ENVIRONMENT VIRIABLE KEYS

        public const string EnvFirebaseStorageUrl = "FIREBASE_STORAGE_URL";

        public const string EnvFirebaseDatabaseUrl = "FIREBASE_DATABSE_URL";

        public const string EnvFirebaseDatabaseSecret = "FIREBASE_DATABASE_SECRET";

        public const string EnvFirebaseApiKey = "FIREBASE_API_KEY";

        public const string EnvDeveloperEmail = "DEVELOPER_EMAIL";

        public const string EnvDeveloperPassword = "DEVELOPER_PASSWORD";

        public const string EnvDeveloperUid = "DEVELOPER_UID";

        public const string EnvConnectionStrings = "SORTMYSTUFF_CONNECTION_STRINGS_AZURE";

        #endregion

        #region AUTHORISATION NAMES

        public const string RoleDeveloper = "DeveloperRole";

        public const string PolicyDeveloper = "DeveloperPolicy";

        #endregion

        #region INDEX NAMES

        public const string IndexAssetContainerId = "AssetContainerIdIndex";

        public const string IndexAssetUserId = "AssetUserIdIndex";

        public const string IndexAssetCategoryId = "AssetCategoryIdIndex";

        public const string IndexCategoryUserId = "CategoryUserIdIndex";

        public const string IndexBaseDetailUserId = "BaseDetailUserIdIndex";

        public const string IndexBaseDetailCategoryId = "BaseDetailCategoryIdIndex";

        public const string IndexDetailUserId = "DetailUserIdIndex";

        public const string IndexDetailBaseDetailId = "DetailBaseDetailIdIndex";

        #endregion

        public const string RootAssetToken = "[RootAssetToken]";

        public const string RootAssetDefaultName = "Assets";
    }
}
