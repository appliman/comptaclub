namespace ComptaClub.Contracts.Models.Exercices
{
    public record CreateExerciceRequest : IRequest<ExerciceData>
    {
        public CreateExerciceRequest()
        {
            Code = "A completer";
            Label = "A completer";
        }

        public CreateExerciceRequest(string code, string label, long initialAmount)
        {
            Code = code;
            Label = label;
            InitialAmount = initialAmount;
        }

        public string Code { get; init; }
        public string Label { get; init; }
        public long InitialAmount { get; init; }
        public bool Active { get; init; } = false;
    }
}
