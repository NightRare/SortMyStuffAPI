namespace SortMyStuffAPI.Utils
{
    public static class ModelRules
    {
        public const int AssetNameLength = 60;
        public const string AssetNameLengthErrorMessage = "The length of the name must be less than 60.";

        public const int CategoryNameLength = 60;
        public const string CategoryNameLengthErrorMessage = "The length of the name must be less than 60.";

        public const int DetailLabelLength = 60;
        public const string DetailLabelLengthErrorMessage = "The length of the label must be less than 60.";

        public const int PagingOffsetMin = 1;
        public const int PagingOffsetMax = 10000;
        public const string PagingOffsetErrorMessage = "Offset parameter must be between 1 to 10000.";

        public const int PagingPageSizeMin = 1;
        public const int PagingPageSizeMax = 10000;
        public const string PagingPageSizeErrorMessage = "PageSize parameter must be between 1 to 10000.";

        public const int UserNameLengthMin = 2;
        public const int UserNameLengthMax = 100;
        public const int UserPasswordLengthMin = 8;
        public const int UserPasswordLengthMax = 32;
    }
}
