namespace SatPortUDAPIServiceNowSyncAgent.DataModel
{
	using Newtonsoft.Json;

	public class UpdateTicketDataModel
	{
		[JsonProperty("incidentID")]
		public string IncidentID { get; set; }

		[JsonProperty("incidentState")]
		public string IncidentState { get; set; }
	}
}