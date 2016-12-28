using Epi.Cloud.Common.BusinessObjects;

namespace Epi.Cloud.Common.BusinessRules
{
    /// <summary>
    /// Represents a validation rules that states that a value is required.
    /// </summary>
    public class ValidateRequired : BusinessRule
    {

        public ValidateRequired(string propertyName)
            : base(propertyName)
        {
            ErrorMessage = propertyName + " is a required field.";
        }

        public ValidateRequired(string propertyName, string errorMessage)
            : base(propertyName)
        {
            ErrorMessage = errorMessage;
        }

        public override bool Validate(BusinessObject businessObject)
        {
            try
            {
                return GetPropertyValue(businessObject).ToString().Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
