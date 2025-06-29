using System.ComponentModel.DataAnnotations;

namespace Mshop.Api.Validations
{
    public class OverYears:ValidationAttribute
    {
        private readonly int year;

        public OverYears(int year)
        {
            this.year = year;
        }
        public override bool IsValid(object? value)
        {
            if(value is DateTime date)
            {
                if(DateTime.Now.Year - date.Year >= year)
                {
                    return true;
                }
            }
            return false;
        }
        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be at least {year} years old.";
        }
    }
}
