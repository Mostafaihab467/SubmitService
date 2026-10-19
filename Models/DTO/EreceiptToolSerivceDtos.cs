using System.Collections.Generic;

namespace EReceiptMiddleware.DTO
{
    public class GenerateUuidResponseDto
    {
        public string CorrelationId { get; set; }
        public string Code { get; set; }
        public string Target { get; set; }
        public string Message { get; set; }
        public List<string> Details { get; set; }
        public string Uuid { get; set; }
        public string UpdatedReceiptJson { get; set; }
    }

    public class GenerateQrCodeResponseDto
    {
        public string CorrelationId { get; set; }
        public string Code { get; set; }
        public string Target { get; set; }
        public string Message { get; set; }
        public List<string> Details { get; set; }
        public string QrCode { get; set; }
        public string QrCodeImageBase64 { get; set; }
    }

    public class ReceiptRequestDto
    {
        public ReceiptDto Receipts { get; set; }
    }

    public class ReceiptDto
    {
        public ReceiptHeader Header { get; set; }
        public DocumentType DocumentType { get; set; }
        public string ReceiptNumber { get; set; }
        public SellerData Seller { get; set; }
        public BuyerData Buyer { get; set; }
        public List<ItemData> ItemData { get; set; }
        public decimal TotalSales { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<TaxTotal> TaxTotals { get; set; }
        public List<DiscountData> ExtraReceiptDiscountData { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class ReceiptHeader
    {
        public string DateTimeIssued { get; set; }
        public string ReceiptNumber { get; set; }
        public string Uuid { get; set; }
        public string PreviousUUID { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }
    }

    public class DocumentType
    {
        public string ReceiptType { get; set; }
        public string TypeVersion { get; set; }
    }

    public class SellerData
    {
        public string Rin { get; set; }
        public string CompanyTradeName { get; set; }
        public string BranchCode { get; set; }
        public AddressData BranchAddress { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string ActivityCode { get; set; }
    }

    public class BuyerData
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string MobileNumber { get; set; }
    }

    public class AddressData
    {
        public string Country { get; set; }
        public string Governate { get; set; }
        public string RegionCity { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
    }

    public class ItemData
    {
        public string InternalCode { get; set; }
        public string Description { get; set; }
        public string ItemType { get; set; }
        public string ItemCode { get; set; }
        public string UnitType { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal NetSale { get; set; }
        public decimal TotalSale { get; set; }
        public decimal Total { get; set; }
        public List<TaxableItem> TaxableItems { get; set; }
    }

    public class TaxableItem
    {
        public string TaxType { get; set; }
        public decimal Amount { get; set; }
        public string SubType { get; set; }
        public decimal Rate { get; set; }
    }

    public class TaxTotal
    {
        public string TaxType { get; set; }
        public decimal Amount { get; set; }
    }

    public class DiscountData
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public decimal Rate { get; set; }
    }
}
