namespace Privy
{
    public static class PrivyEventType
    {
        public const string Ready = "privy:iframe:ready";
        public const string Create = "privy:wallet:create";
        public const string CreateAdditional = "privy:wallet:create-additional";
        public const string Connect = "privy:wallet:connect";
        public const string Recover = "privy:wallet:recover";
        public const string Rpc = "privy:wallet:rpc";
        public const string SetPassword = "privy:wallet:set-recovery-password";
    }
}