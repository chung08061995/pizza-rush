namespace DraftUtils.IAP
{
    /// <summary>
    /// Kết quả của một lần purchase.
    /// </summary>
    public class IAPPurchaseResult
    {
        /// <summary>Mua thành công hay không.</summary>
        public bool IsSuccess { get; set; }

        /// <summary>ID product vừa mua.</summary>
        public string ProductId { get; set; }

        /// <summary>Mã lỗi nếu thất bại.</summary>
        public IAPFailureReason FailureReason { get; set; }

        /// <summary>Thông tin lỗi chi tiết.</summary>
        public string ErrorMessage { get; set; }

        /// <summary>Receipt string (dùng cho server validation).</summary>
        public string Receipt { get; set; }

        /// <summary>Transaction ID từ store.</summary>
        public string TransactionId { get; set; }

        public static IAPPurchaseResult Success(string productId, string receipt = "", string transactionId = "")
        {
            return new IAPPurchaseResult
            {
                IsSuccess = true,
                ProductId = productId,
                Receipt = receipt,
                TransactionId = transactionId,
                FailureReason = IAPFailureReason.None
            };
        }

        public static IAPPurchaseResult Failure(string productId, IAPFailureReason reason, string errorMessage = "")
        {
            return new IAPPurchaseResult
            {
                IsSuccess = false,
                ProductId = productId,
                FailureReason = reason,
                ErrorMessage = errorMessage
            };
        }
    }

    /// <summary>
    /// Lý do purchase thất bại.
    /// </summary>
    public enum IAPFailureReason
    {
        None,
        /// <summary>IAP chưa được khởi tạo.</summary>
        NotInitialized,
        /// <summary>Không tìm thấy product.</summary>
        ProductUnavailable,
        /// <summary>User huỷ mua.</summary>
        UserCancelled,
        /// <summary>Lỗi thanh toán (thẻ, mạng...).</summary>
        PaymentDeclined,
        /// <summary>Đang xử lý purchase khác.</summary>
        PurchasePending,
        /// <summary>Product đã sở hữu (non-consumable).</summary>
        AlreadyOwned,
        /// <summary>Lỗi không xác định.</summary>
        Unknown
    }
}
