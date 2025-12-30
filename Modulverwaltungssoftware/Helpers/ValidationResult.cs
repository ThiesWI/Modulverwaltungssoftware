using System.Collections.Generic;

namespace Modulverwaltungssoftware.Helpers
{
    /// <summary>
    /// Repräsentiert das Ergebnis einer Validierung mit feldspezifischen Fehlern
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public Dictionary<string, string> FieldErrors { get; set; }
        public string GlobalMessage { get; set; }

        public ValidationResult()
        {
            IsValid = true;
            FieldErrors = new Dictionary<string, string>();
            GlobalMessage = string.Empty;
        }

        public void AddError(string fieldName, string errorMessage)
        {
            IsValid = false;
            if (!FieldErrors.ContainsKey(fieldName))
            {
                FieldErrors.Add(fieldName, errorMessage);
            }
        }

        public bool HasError(string fieldName)
        {
            return FieldErrors.ContainsKey(fieldName);
        }

        public string GetError(string fieldName)
        {
            return FieldErrors.ContainsKey(fieldName) ? FieldErrors[fieldName] : string.Empty;
        }
    }
}
