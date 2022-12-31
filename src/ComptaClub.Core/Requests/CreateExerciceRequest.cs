
namespace ComptaClub.Requests
{
    public record CreateExerciceRequest : IRequest<Datas.ExerciceData>
    {
        public CreateExerciceRequest()
        {
            Code = "A completer";
            Label = "A completer";
        }

        public CreateExerciceRequest(string code, string label, int startDate, int endDate, long initialAmount, bool active = true)
        {
            this.Code= code;
            this.Label= label;
            this.StartDate= startDate;
            this.EndDate= endDate;
            this.InitialAmount= initialAmount;
            this.Active= active;
        }

        public string Code { get; init; }
        public string Label { get; init; }
        public int StartDate { get; init; }
        public int EndDate { get; init; }
        public long InitialAmount { get; init; }
        public bool Active { get; init; }
    }
}
