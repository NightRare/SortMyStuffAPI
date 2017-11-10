using System;

namespace SortMyStuffAPI.Utils
{
    public static class ApiStrings
    {
        public const string ERROR_COMMON_MSG = "A server error occurred.";

        public const string HTTP_GET = "GET";

        public const string HTTP_POST = "POST";

        public const string HTTP_PUT = "PUT";

        public const string HTTP_DELETE = "DELETE";

        public const string HTTP_PATCH = "PATCH";

        public const string PARAMETER_DESC = "desc";

        public const string PARAMETER_OP_EQUAL = "eq";

        public const string PARAMETER_OP_GREATERTHAN = "gt";

        public const string PARAMETER_OP_LESSTHAN = "lt";

        public const string PARAMETER_OP_GREATERTHAN_OR_EQUAL = "gte";

        public const string PARAMETER_OP_LESSTHAN_OR_EQUAL = "lte";

        public const string FORM_REL = "form";

        public const string FORM_EDIT_REL = "edit-form";

        public const string FORM_CREATE_REL = "create-form";

        public const string FORM_QUERY_REL = "query-form";


        #region ENVIRONMENT VIRIABLE KEYS

        public const string ENV_FIREBASE_STORAGE_URL = "FIREBASE_STORAGE_URL";

        public const string ENV_FIREBASE_DATABASE_URL = "FIREBASE_DATABSE_URL";

        public const string ENV_FIREBASE_DATABASE_SECRET = "FIREBASE_DATABASE_SECRET";

        public const string ENV_FIREBASE_API_KEY = "FIREBASE_API_KEY";

        public const string ENV_FIREBASE_AUTH_EMAIL = "FIREBASE_AUTH_EMAIL";

        public const string ENV_FIREBASE_AUTH_PASSWORD = "FIREBASE_AUTH_PASSWORD";

        #endregion
    }
}
