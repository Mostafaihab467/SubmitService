namespace SubmitServ
{
    internal class UpdateSubmitResponseDTO
    {
        public long? LogID { get; set; }
        public long? EReceiptID { get; set; }
        public string ETAUUID { get; set; }
        public string ETAQRCode { get; set; }
        public string ETAResponsePayload { get; set; }
        public string Error { get; set; }
    }
}