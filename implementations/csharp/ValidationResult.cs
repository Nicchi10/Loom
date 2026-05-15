using Loom.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    /// <summary>
    /// Implements IValidationResult
    /// </summary>
    public class ValidationResult : Interfaces.IValidationResult
    {
        private readonly bool _isValid;
        private readonly List<string> _errors = new List<string>();

        public bool IsValid => _isValid;
        public IEnumerable<string> Erros => _errors;

        /// <summary>
        /// Validator constructor
        /// </summary>
        /// <param name="isValid"> Boolean </param>
        /// <param name="errors"> List of String </param>
        public ValidationResult(bool isValid, List<string> errors = null)
        {
            _isValid = isValid;
            if (errors != null)
            {
                _errors.AddRange(errors);
            }
        }

        /// <summary>
        /// Success
        /// </summary>
        /// <returns> No error (True) </returns>
        public static ValidationResult Success()
        {
            return new ValidationResult(true);
        }

        /// <summary>
        /// Failure
        /// </summary>
        /// <param name="errors"> List of String </param>
        /// <returns> False + Errors </returns>
        public static ValidationResult Failure(params string[] errors)
        {
            return new ValidationResult(false, errors.ToList());
        }
    }
}