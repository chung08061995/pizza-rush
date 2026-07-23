// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("efelpAa+FQjkhRfVfTrTC0T5oX60264CbtyQgkLMKbPRZc6EXcmhmgkDhlNnREkXFF1y7EiSEQUze+irDjxZn/5E1mShOldCV92caSwMGpX5aELelNUtQQu9vkoim2CWj7HUb4uq1AKPHNSBmAZ7oO4UKrEh1Pw8lU3k/I52uycw9LXOAqCJVck2Zx02qdqECp5TdsYga+YHzxt7McvVBxQsKOBR4fSPC66mbZ5PctGfoz25ItFsWi6iioetaZAcq2F8FS+96llu7ePs3G7t5u5u7e3sNST7NsImteCEfeBs5oXP3CcqWkQOmi1Ik+ko3G7tztzh6uXGaqRqG+Ht7e3p7O+5AX/U+30duzrA4vjnKYQp35ZRfVv3Dwkz1JNmue7v7ezt");
        private static int[] order = new int[] { 8,11,13,10,9,7,7,9,12,9,11,11,12,13,14 };
        private static int key = 236;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
