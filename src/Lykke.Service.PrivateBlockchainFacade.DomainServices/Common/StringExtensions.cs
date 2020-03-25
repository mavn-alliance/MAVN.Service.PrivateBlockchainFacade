using System.Text.RegularExpressions;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices.Common
{
    public static class StringExtensions
    {
        private static string TransactionHashRegularExpression = @"0x[a-fA-F0-9]{64}";

        public static bool IsValidTransactionHash(this string src)
        {
            if (string.IsNullOrEmpty(src))
                return false;
            
            var regex = new Regex(TransactionHashRegularExpression);
            
            return regex.Match(src).Success;
        }
    }
}
