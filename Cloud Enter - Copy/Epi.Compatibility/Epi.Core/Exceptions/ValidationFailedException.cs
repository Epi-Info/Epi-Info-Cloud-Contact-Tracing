using System;

namespace Epi
{
    /// <summary>
    /// Exception thrown when validation fails
    /// </summary>
    [Serializable]
    public class ValidationFailedException : GeneralException
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ValidationFailedException()
        {
        }
    }
}