
namespace ComptaClub.Requests
{
    public record CreateExerciceRequest : IRequest<Datas.ExerciceData>
    {
        public CreateExerciceRequest()
        {
            this.Code = "A completer";
            this.Label = "A completer";
            this.StartDate = DateTime.Today.FirstDateOfCurrentYear();
			this.EndDate = DateTime.Today.LastDateOfCurrentYear();
		}

		public CreateExerciceRequest(string code, string label, int startDate, int endDate, long initialAmount)
        {
            this.Code= code;
            this.Label= label;
            this.StartDate= startDate;
            this.EndDate= endDate;
            this.InitialAmount= initialAmount;
        }

        public string Code { get; init; }
        public string Label { get; init; }
        public int StartDate { get; init; }
        public int EndDate { get; init; }
        public long InitialAmount { get; init; }
        public bool Active { get; init; } = false;
    }
}
