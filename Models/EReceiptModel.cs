using System;

namespace SubmitServ.Models
{
    public class receiptsModel
    {
        public EReceiptModel[] receipts { get; set; }
    }
    public class EReceiptModel
    {
        private string _paymentMethod;
        public header header { get; set; }
        public documentType documentType { get; set; }
        public seller seller { get; set; }
        public buyer buyer { get; set; }
        public itemData[] itemData { get; set; }
        public decimal? totalSales { get; set; }
        public decimal? netAmount { get; set; }
        //public decimal? feesAmount { get; set; }
        public decimal? totalAmount { get; set; }
        public taxTotals[] taxTotals { get; set; }
        public string paymentMethod
        {
            get { return _paymentMethod; }
            set { _paymentMethod = "C"; }
        }
    }

    public class header
    {
        private decimal? _exchangeRate;

        // Using string to ensure ISO 8601 format without timezone offset
        // Format: yyyy-MM-ddTHH:mm:ss
        public string dateTimeIssued { get; set; }
        public string receiptNumber { get; set; }
        public string uuid { get; set; }
        public string previousUUID { get; set; }
        public string currency { get; set; }
        public decimal? exchangeRate
        {
            get { return _exchangeRate; }
            set { _exchangeRate = 0; }
        }
    }
    public class documentType
    {
        public string receiptType { get; set; }
        public string typeVersion { get; set; }
    }
    public class seller
    {
        public string rin { get; set; }
        public string companyTradeName { get; set; }
        public string branchCode { get; set; }
        public branchAddress branchAddress { get; set; }
        public string deviceSerialNumber { get; set; }
        public string activityCode { get; set; }
    }
    public class branchAddress
    {
        public string country { get; set; }
        public string governate { get; set; }
        public string regionCity { get; set; }
        public string street { get; set; }
        public string buildingNumber { get; set; }
    }
    public class buyer
    {
        public string type { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }
    public class itemData
    {
        public string internalCode { get; set; }
        //mandatory
        public string description { get; set; }
        public string itemType { get; set; }
        public string itemCode { get; set; }
        public string unitType { get; set; }
        public decimal? quantity { get; set; }
        public decimal? unitPrice { get; set; }
        public decimal? netSale { get; set; }
        public decimal? totalSale { get; set; }
        public decimal? total { get; set; }
        public itemDiscountData[] itemDiscountData { get; set; }
        public taxableItems[] taxableItems { get; set; }
    }
    public class itemDiscountData
    {
        public decimal? amount { get; set; }
        public string description { get; set; }
        public decimal? rate { get; set; }
    }
    public class taxableItems
    {
        public string taxType { get; set; }
        public decimal? amount { get; set; }
        public string subType { get; set; }
        public decimal? rate { get; set; }
    }
    public class taxTotals
    {
        public string taxType { get; set; }
        public decimal? amount { get; set; }
    }
}
