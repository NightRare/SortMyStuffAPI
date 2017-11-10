using System;

namespace SortMyStuffAPI.Utils
{
    public static class ApiStrings
    {
        public const string ErrorCommonMsg = "A server error occurred.";

        public const string ErrorCommonDetail = "No further detail.";

        public const string HttpGet = "GET";

        public const string HttpPost = "POST";

        public const string HttpPut = "PUT";

        public const string HttpDelete = "DELETE";

        public const string HttpPatch = "PATCH";

        public const string ParameterDesc = "desc";

        public const string ParameterOpEqual = "eq";

        public const string ParameterOpGreaterThan = "gt";

        public const string ParameterOpLessThan = "lt";

        public const string ParameterOpGreaterThanOrEqual = "gte";

        public const string ParameterOpLessThanOrEqual = "lte";

        public const string FormRel = "form";

        public const string FormEditRel = "edit-form";

        public const string FormCreateRel = "create-form";

        public const string FormQueryRel = "query-form";


        #region ENVIRONMENT VIRIABLE KEYS

        public const string EnvFirebaseStorageUrl = "FIREBASE_STORAGE_URL";

        public const string EnvFirebaseDatabaseUrl = "FIREBASE_DATABSE_URL";

        public const string EnvFirebaseDatabaseSecret = "FIREBASE_DATABASE_SECRET";

        public const string EnvFirebaseApiKey = "FIREBASE_API_KEY";

        public const string EnvFirebaseAuthEmail = "FIREBASE_AUTH_EMAIL";

        public const string EnvFirebaseAuthPassword = "FIREBASE_AUTH_PASSWORD";

        #endregion
    }
}
