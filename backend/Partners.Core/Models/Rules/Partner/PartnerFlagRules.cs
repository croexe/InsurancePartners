namespace Partners.Core.Models.Rules.Partner
{
    public static class PartnerFlagRules
    {
        private const int MaxPolicyCountBeforeFlag = 5;
        private const decimal MaxPolicyAmountBeforeFlag = 5000m;

        public static bool IsFlagged(int policyCount, decimal totalAmount) =>
            policyCount > MaxPolicyCountBeforeFlag || totalAmount > MaxPolicyAmountBeforeFlag;
    }
}
