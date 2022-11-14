using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace ComptaClub.Requests
{
    public record CreateExercice : IRequest<Models.Exercice>
    {
        public CreateExercice(string code, string label, int startDate, int endDate, long initialAmount)
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
    }
}
