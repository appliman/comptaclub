namespace ComptaClub.Requests.Exercices
{
    public record CreateExerciceRequest : IRequest<ExerciceData>
    {
        public CreateExerciceRequest()
        {
            Code = "A completer";
            Label = "A completer";
            StartDate = DateTime.Today.FirstDateOfCurrentYear();
            EndDate = DateTime.Today.LastDateOfCurrentYear();
        }

        public CreateExerciceRequest(string code, string label, int startDate, int endDate, long initialAmount)
        {
            Code = code;
            Label = label;
            StartDate = startDate;
            EndDate = endDate;
            InitialAmount = initialAmount;
        }

        public string Code { get; init; }
        public string Label { get; init; }
        public int StartDate { get; init; }
        public int EndDate { get; init; }
        public long InitialAmount { get; init; }
        public bool Active { get; init; } = false;
    }
}
