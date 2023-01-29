namespace ComptaClub.Blazor.Pages.Components;

public partial class DateLabel
{
    [Parameter]
    public DateTime Value { get; set; }

    [Parameter]
    public string Format { get; set; } = "ddd MMM yy";

    MarkupString MomentValue
    {
        get
        {
            var date = Value;
            var ci = new System.Globalization.CultureInfo("fr-FR");
            var result = string.Format(ci,$"{{0:{Format}}}", date);
            if (Format.Equals("fromNow", StringComparison.InvariantCultureIgnoreCase))
            {
                result = FromNow(date);
            }

            return new MarkupString(result);
        }
    }

    string LongDateFormat
    {
        get
        {
            return string.Format("le {0:dddd dd MMMM yyyy à HH:mm:ss}", Value);
        }
    }

    string FromNow(DateTime start)
    {
        TimeSpan sincets = DateTime.Now - start;
        int sinceTotalDays = Convert.ToInt32(Math.Floor(sincets.TotalDays));
        int sinceTotalHours = Convert.ToInt32(Math.Floor(sincets.TotalHours));
        int sinceTotalMinutes = Convert.ToInt32(Math.Floor(sincets.TotalMinutes));
        if (sinceTotalDays == 0 && start.Day == DateTime.Now.Day)
        {
            if (sinceTotalHours == 0)
            {
                return string.Format("Il y a {0} minute{1}", sinceTotalMinutes, sinceTotalMinutes > 1 ? "s" : "");
            }
            else if (sinceTotalHours < 2)
            {
                return string.Format("Il y a 1 heure et {0} minute{1}", sinceTotalMinutes - 60, sinceTotalMinutes - 60 > 1 ? "s" : "");
            }
            else if (sinceTotalHours < 12)
            {
                return string.Format("Il y a plus de {0} heure{1}", sinceTotalHours, sinceTotalHours > 1 ? "s" : "");
            }
            else
            {
                return "Aujourd'hui";
            }
        }
        else if (sinceTotalDays == 0 || sinceTotalDays == 1)
        {
            return string.Format("hier à {0:HH}h{0:mm}", start);
        }
        else if (sinceTotalDays == 2)
        {
            return "avant hier";
        }
        else if (sinceTotalDays < 2)
        {
            return string.Format("il y a {1} jours,{0:dddd dd MMMM} ", start, Math.Abs(sincets.Days));
        }
        else if (sinceTotalDays < 7)
        {
            return string.Format("moins de 7 jours,{0:dddd dd MMMM} ", start, Math.Abs(sincets.Days));
        }
        else if (sinceTotalDays < 30)
        {
            return string.Format("moins d'un mois,{0:dddd dd MMMM} ", start);
        }
        else if (sinceTotalDays < 60)
        {
            return string.Format("plus d'un mois,{0:dddd dd MMMM} ", start);
        }
        else if (sinceTotalDays < 90)
        {
            return string.Format("plus de 2 mois,{0:dddd dd MMMM} ", start);
        }
        else if (sinceTotalDays < 120)
        {
            return string.Format("plus de 3 mois,{0:dddd dd MMMM} ", start);
        }

        return string.Format("{0:D}", start);
    }
}