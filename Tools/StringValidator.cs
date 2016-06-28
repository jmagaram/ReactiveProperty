using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tools {
    public class StringValidator {
        public StringValidator(
            bool isRequired = false,
            int? minLength = null,
            int? maxLength = null,
            string pattern = null) {
            MinLength = minLength;
            MaxLength = maxLength;
            IsRequired = isRequired;
            Pattern = pattern;
            Regex = string.IsNullOrWhiteSpace(pattern) ? null : new Regex(pattern);
        }

        public IEnumerable<StringError> Validate(string i) {
            if (string.IsNullOrWhiteSpace(i)) {
                if (IsRequired) {
                    yield return StringError.IsRequired;
                }
            }
            else {
                if (MinLength != null && i.Length < MinLength) {
                    yield return StringError.TooShort;
                }
                if (MaxLength != null && i.Length > MaxLength) {
                    yield return StringError.TooLong;
                }
                if (Regex != null) {
                    if (!Regex.IsMatch(i)) {
                        yield return StringError.PatternMismatch;
                    }
                }
            }
        }

        public int? MinLength { get; }
        public int? MaxLength { get; }
        public string Pattern { get; }
        public Regex Regex { get; }
        public bool IsRequired { get; }
    }
}
