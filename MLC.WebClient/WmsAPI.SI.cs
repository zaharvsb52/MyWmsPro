namespace MLC.WebClient
{
    public partial class WmsAPI
    {
        public void SetOwbReserve(int owbId)
        {
            WithTransaction("integrationInSetOrderReserve")
                .AddParameter("owbId", owbId)
                .Process<dynamic>();
        }
    }
}
