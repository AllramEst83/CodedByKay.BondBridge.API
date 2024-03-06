namespace CodedByKay.BondBridge.API.Models
{
    public class TokenValidationConstants
    {
        public static class Roles
        {
            public const string AdminAccess = "admin_access";
            public const string CommonUserAccess = "common_user_access";
            public const string AppAccess = "app_access";
            public const string Role = "rol";
            public const string Id = "id";

        }

        public static class Policies
        {
            public const string CodedByKayBondBridgeApiAdmin = "CodedByKay.BondBridge.API.Admin";
            public const string CodedByKayBondBridgeApiCommonUser = "CodedByKay.BondBridge.API.CommonUser";
            public const string CodedByKayBondBridgeApiAppAccess = "CodedByKay.BondBridge.API.AppAccess";
        }
    }
}
