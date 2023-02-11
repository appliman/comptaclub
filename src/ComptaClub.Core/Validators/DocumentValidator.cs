namespace ComptaClub.Validators;

internal class DocumentValidator : FluentValidation.AbstractValidator<Datas.DocumentData>
{
	public DocumentValidator()
	{
        RuleFor(i => i.Id).ValidGuid();
        RuleFor(i => i.FileName).NotEmpty().NotNull().WithMessage("Le document doit avoir un nom de fichier");
        RuleFor(i => i.MimeType).NotEmpty().NotNull().WithMessage("Le document doit comporter un type de contenu standard");
    }
}
