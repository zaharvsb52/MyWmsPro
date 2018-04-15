namespace wmsMLC.APS.wmsSI.Messages
{
    public class ReserveResponse
    {
        public decimal? OrderId { get; set; }
        public ReserveStates ReserveState { get; set; }
    }
}