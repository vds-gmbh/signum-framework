using System.Collections;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Signum.Utilities.Reflection;
using System.Globalization;
using System.IO;

namespace Signum.Entities.Validation;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public abstract class ValidatorAttribute : Attribute
{


    public Func<ModifiableEntity, bool>? IsApplicable;
    public Func<string>? ErrorMessage { get; set; }

    public string? UnlocalizableErrorMessage
    {
        get { return ErrorMessage == null ? null : ErrorMessage(); }
        set { ErrorMessage = () => value!; }
    }

    public int Order { get; set; }

    public bool DisabledInModelBinder { get; set; } = false;

    //Descriptive information that continues the sentence: The property should {HelpMessage}
    //Used for documentation purposes only
    public abstract string HelpMessage { get; }

    public string? Error(ModifiableEntity entity, PropertyInfo property, object? value)
    {
        if (DisabledInModelBinder && Validator.InModelBinder)
            return null;

        if (IsApplicable != null && !IsApplicable(entity))
            return null;

        string? defaultError = OverrideError(value);

        if (defaultError == null)
            return null;

        string error = ErrorMessage == null ? defaultError : ErrorMessage();
        if (error != null)
            error = error.FormatWith(property.NiceName());

        return error;
    }

    public virtual bool IsCompatibleWith(PropertyInfo pi)
    {
        return true;
    }

    /// <summary>
    /// When overriden, validates the value against this validator rule
    /// </summary>
    /// <param name="value"></param>
    /// <returns>returns an string with the error message, using {0} if you want the property name to be inserted</returns>
    protected abstract string? OverrideError(object? value);
}

public class NotNullValidatorAttribute : ValidatorAttribute
{
    public bool Disabled { get; set; }

    protected override string? OverrideError(object? obj)
    {
        if (Disabled)
            return null;

        if (obj == null || obj is string s && s == "")
            return ValidationMessage._0IsNotSet.NiceToString();

        return null;
    }

    public override string HelpMessage => ValidationMessage.BeNotNull.NiceToString();

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        return !pi.PropertyType.IsValueType || pi.PropertyType.IsNullable();
    }
}

public static class NotNullValidatorExtensions
{
    public static string? IsSetOnlyWhen(this (PropertyInfo pi, object? nullableValue) tuple, bool shouldBeSet)
    {
        var isNull = tuple.nullableValue == null ||
            tuple.nullableValue is string s && string.IsNullOrEmpty(s) ||
            tuple.nullableValue is ICollection col && col.Count == 0;

        if (isNull && shouldBeSet)
            return ValidationMessage._0IsNotSet.NiceToString(tuple.pi.NiceName());

        else if (!isNull && !shouldBeSet)
            return ValidationMessage._0ShouldBeNull.NiceToString(tuple.pi.NiceName());

        return null;
    }
}


public class StringLengthValidatorAttribute : ValidatorAttribute
{
    bool? allowLeadingSpaces;
    public bool AllowLeadingSpaces
    {
        get { return allowLeadingSpaces ?? MultiLine; }
        set { this.allowLeadingSpaces = value; }
    }

    bool? allowTrailingSpaces;
    public bool AllowTrailingSpaces
    {
        get { return allowTrailingSpaces ?? MultiLine; }
        set { this.allowTrailingSpaces = value; }
    }

    public bool MultiLine { get; set; }

    int min = -1;
    public int Min
    {
        get { return min; }
        set { min = value; }
    }

    int max = -1;
    public int Max
    {
        get { return max; }
        set { max = value; }
    }

    protected override string? OverrideError(object? value)
    {
        string val = (string)value!;

        if (string.IsNullOrEmpty(val))
            return null;

        if (!MultiLine && (val.Contains('\n') || val.Contains('\r')))
            return ValidationMessage._0ShouldHaveJustOneLine.NiceToString();

        if (!AllowLeadingSpaces && Regex.IsMatch(val, @"^\s+"))
            return ValidationMessage._0ShouldNotHaveInitialSpaces.NiceToString();

        if (!AllowTrailingSpaces && Regex.IsMatch(val, @"\s+$"))
            return ValidationMessage._0ShouldNotHaveFinalSpaces.NiceToString();

        if (min == max && min != -1 && val.Length != min)
            return ValidationMessage.TheLenghtOf0HasToBeEqualTo1.NiceToString("{0}", min);

        if (min != -1 && val.Length < min)
            return ValidationMessage.TheLengthOf0HasToBeGreaterOrEqualTo1.NiceToString("{0}", min);

        if (max != -1 && val.Length > max)
            return ValidationMessage.TheLengthOf0HasToBeLesserOrEqualTo1.NiceToString("{0}", max);

        return null;
    }

    public override string HelpMessage
    {
        get
        {
            string result =
                min != -1 && max != -1 ? ValidationMessage.HaveBetween0And1Characters.NiceToString().FormatWith(min, max) :
                min != -1 ? ValidationMessage.HaveMinimum0Characters.NiceToString().FormatWith(min) :
                max != -1 ? ValidationMessage.HaveMaximum0Characters.NiceToString().FormatWith(max) :
                MultiLine ? ValidationMessage.BeAMultilineString.NiceToString() :
                ValidationMessage.BeAString.NiceToString();

            return result;
        }
    }

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        return pi.PropertyType == typeof(string);
    }
}


public abstract class RegexValidatorAttribute : ValidatorAttribute
{
    Regex[] regexList;
    public RegexValidatorAttribute(Regex regex)
    {
        this.regexList = new[] { regex };
    }

    public RegexValidatorAttribute(Regex[] regex)
    {
        this.regexList = regex;
    }

    public RegexValidatorAttribute(string regexExpresion)
    {
        this.regexList = new[] { new Regex(regexExpresion) };
    }

    public abstract string FormatName
    {
        get;
    }

    protected override string? OverrideError(object? value)
    {
        string? str = (string?)value;
        if (string.IsNullOrEmpty(str))
            return null;

        if (regexList.Any(a => a.IsMatch(str)))
            return null;

        return ValidationMessage._0DoesNotHaveAValid1Format.NiceToString().FormatWith("{0}", FormatName);
    }

    public override string HelpMessage => ValidationMessage.HaveValid0Format.NiceToString().FormatWith(FormatName);

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        return pi.PropertyType == typeof(string);
    }
}

public class EMailValidatorAttribute : RegexValidatorAttribute
{
    public static Regex EmailRegex = new Regex(
                      @"^(([^<>()[\]\\.,;:\s@\""]+"
                    + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                    + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                    + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                    + @"[a-zA-Z]{2,}))$", RegexOptions.IgnoreCase);

    public EMailValidatorAttribute()
        : base(EmailRegex)
    {
    }

    public override string FormatName
    {
        get { return "e-Mail"; }
    }
}

public class TelephoneValidatorAttribute : RegexValidatorAttribute
{

    public static Regex TelephoneRegex = new Regex(@"^[\p{Nd}+\-/() ]+$");

    public TelephoneValidatorAttribute()
        : base(TelephoneRegex)
    {
    }

    public override string FormatName
    {
        get { return ValidationMessage.Telephone.NiceToString(); }
    }
}
public class AlphanumericOnlyValidatorAttribute : RegexValidatorAttribute
{
    public static Regex AlphanumericOnlyRegex = new Regex($@"[A-Za-z0-9]");

    public AlphanumericOnlyValidatorAttribute()
        : base(AlphanumericOnlyRegex)
    {
    }

    public override string FormatName
    {
        get { return ValidationMessage._0IsNotAllowed.NiceToString(@"Special characters (-_#+%&...)"); }
    }
}

public class MultipleTelephoneValidatorAttribute : RegexValidatorAttribute
{
    public static Regex MultipleTelephoneRegex = new Regex(@"^[\p{Nd}+\-/() ](,\s*[\p{Nd}+\-/() ])*");

    public MultipleTelephoneValidatorAttribute()
        : base(MultipleTelephoneRegex)
    {
    }

    public override string FormatName
    {
        get { return ValidationMessage.Telephone.NiceToString(); }
    }
}

public class IdentifierValidatorAttribute : RegexValidatorAttribute
{
    public static Regex PascalAscii = new Regex(@"^[A-Z[_a-zA-Z0-9]*$");
    public static Regex Ascii = new Regex(@"^[_a-zA-Z[_a-zA-Z0-9]*$");
    public static Regex International = new Regex(@"^[_\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nl}][_\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nl}\p{Nd}]*$");

    public IdentifierType type;
    public IdentifierValidatorAttribute(IdentifierType type)
           : base(
                 type == IdentifierType.PascalAscii ? PascalAscii :
                 type == IdentifierType.Ascii ? Ascii :
                 type == IdentifierType.International ? International :
                 throw new UnexpectedValueException(type)
                 )
    {
        this.type = type;
    }

    public override string FormatName => this.type.ToString();
}

public enum IdentifierType
{
    PascalAscii,
    Ascii,
    International
}

public class NumericTextValidatorAttribute : RegexValidatorAttribute
{
    public static Regex NumericTextRegex = new Regex(@"^[\p{Nd}]*$");

    public NumericTextValidatorAttribute()
        : base(NumericTextRegex)
    {
    }

    public override string FormatName
    {
        get { return ValidationMessage.Numeric.NiceToString(); }
    }
}

public class URLValidatorAttribute : RegexValidatorAttribute
{
    public static Regex AbsoluteUrlRegex = new Regex(
          "^(https?://)"
        + "(([0-9a-z_!~*'().&=+$%-]+: )?[0-9a-z_!~*'().&=+$%-]+@)?" //user@
        + @"(([0-9]{1,3}\.){3}[0-9]{1,3}" // IP- 199.194.52.184
        + "|" // allows either IP or domain
        + @"([0-9a-z_!~*'()-]+\.)*" // tertiary domain(s)- www.
        + @"([0-9a-z][0-9a-z-]{0,61})?[0-9a-z]" // second level domain
        + @"(\.[a-z]{2,6})?)" // first level domain- .com or .museum
        + "(:[0-9]{1,4})?" // port number- :80
        + "[/0-9a-z_!~*'().;?:@&=+$,%#-|]+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static Regex SiteRelativeRegex = new Regex(
        "^/|(/[0-9a-z_!~*'().;?:@&=+$,%#-]+)+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static Regex AspNetRelativeRegex = new Regex(
        "^~(/|(/[0-9a-z_!~*'().;?:@&=+$,%#-]+)+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static Regex DocumentRelativeRegex = new Regex(
        "^[0-9a-z_!~*'().;?:@&=+$,%#-](/[0-9a-z_!~*'().;?:@&=+$,%#-]+)+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public URLValidatorAttribute(bool absolute = true, bool siteRelative = false, bool aspNetSiteRelative = false, bool documentRelative = false)
        : base(new[]{
                absolute ? AbsoluteUrlRegex: null,
                siteRelative ? SiteRelativeRegex: null,
                aspNetSiteRelative ? AspNetRelativeRegex: null,
                documentRelative ? DocumentRelativeRegex: null,
        }.NotNull().ToArray())
    {
    }

    public override string FormatName
    {
        get { return "URL"; }
    }
}

public class AzureStorageCollectionNameValidationAttribute : RegexValidatorAttribute
{
    static Regex CollectionNameRegex = new Regex(@"^[a-z\-]+");

    public AzureStorageCollectionNameValidationAttribute() : base(CollectionNameRegex)
    {
    }

    public override string FormatName => "Azure Storage Collection Name";
}

public class FileNameValidatorAttribute : ValidatorAttribute
{
    public static char[] InvalidCharts = Path.GetInvalidPathChars();

    static Regex invalidChartsRegex = new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");

    public FileNameValidatorAttribute()
    {
    }

    public string FormatName => ValidationMessage.FileName.NiceToString();

    public override string HelpMessage => ValidationMessage.HaveValid0Format.NiceToString().FormatWith(FormatName);

    protected override string? OverrideError(object? value)
    {
        string? str = (string?)value;

        if (str == null)
            return null;

        if (str.IndexOfAny(InvalidCharts) == -1)
            return null;

        return ValidationMessage._0DoesNotHaveAValid1Format.NiceToString().FormatWith("{0}", FormatName);
    }

    public static string RemoveInvalidCharts(string a)
    {
        return invalidChartsRegex.Replace(a, "");
    }

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        return pi.PropertyType == typeof(string);
    }
}

public class DecimalsValidatorAttribute : ValidatorAttribute
{
    public byte DecimalPlaces { get; set; }

    public DecimalsValidatorAttribute()
    {
        DecimalPlaces = 2;
    }

    public DecimalsValidatorAttribute(byte decimalPlaces)
    {
        this.DecimalPlaces = decimalPlaces;
    }

    protected override string? OverrideError(object? value)
    {
        if (value == null)
            return null;

        if (value is decimal && Math.Round((decimal)value, DecimalPlaces) != (decimal)value)
        {
            return ValidationMessage._0HasMoreThan1DecimalPlaces.NiceToString("{0}", DecimalPlaces);
        }

        return null;
    }

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        var type = pi.PropertyType.UnNullify();
        return type == typeof(decimal);
    }

    public override string HelpMessage => ValidationMessage.Have0Decimals.NiceToString().FormatWith(DecimalPlaces);
}


public class NumberIsValidatorAttribute : ValidatorAttribute
{
    public ComparisonType ComparisonType;
    public IComparable number;

    public NumberIsValidatorAttribute(ComparisonType comparison, float number)
    {
        this.ComparisonType = comparison;
        this.number = number;
    }

    public NumberIsValidatorAttribute(ComparisonType comparison, double number)
    {
        this.ComparisonType = comparison;
        this.number = number;
    }

    public NumberIsValidatorAttribute(ComparisonType comparison, byte number)
    {
        this.ComparisonType = comparison;
        this.number = number;
    }

    public NumberIsValidatorAttribute(ComparisonType comparison, short number)
    {
        this.ComparisonType = comparison;
        this.number = number;
    }

    public NumberIsValidatorAttribute(ComparisonType comparison, int number)
    {
        this.ComparisonType = comparison;
        this.number = number;
    }

    public NumberIsValidatorAttribute(ComparisonType comparison, long number)
    {
        this.ComparisonType = comparison;
        this.number = number;
    }

    protected override string? OverrideError(object? value)
    {
        if (value == null)
            return null;

        IComparable val = (IComparable)value;

        if (number.GetType() != value.GetType())
            number = (IComparable)Convert.ChangeType(number, value.GetType()); // made just once

        bool ok = (ComparisonType == ComparisonType.EqualTo && val.CompareTo(number) == 0) ||
                  (ComparisonType == ComparisonType.DistinctTo && val.CompareTo(number) != 0) ||
                  (ComparisonType == ComparisonType.GreaterThan && val.CompareTo(number) > 0) ||
                  (ComparisonType == ComparisonType.GreaterThanOrEqualTo && val.CompareTo(number) >= 0) ||
                  (ComparisonType == ComparisonType.LessThan && val.CompareTo(number) < 0) ||
                  (ComparisonType == ComparisonType.LessThanOrEqualTo && val.CompareTo(number) <= 0);

        if (ok)
            return null;

        return ValidationMessage._0ShouldBe12.NiceToString().FormatWith("{0}", ComparisonType.NiceToString().ToLower(), number.ToString());
    }

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        var type = pi.PropertyType.UnNullify();
        return ReflectionTools.IsNumber(type);
    }

    public override string HelpMessage => ValidationMessage.Be0.NiceToString(ComparisonType.NiceToString().ToLower() + " " + number.ToString());
}

//Not using C intervals to please user!
public class NumberBetweenValidatorAttribute : ValidatorAttribute
{
    IComparable min;
    IComparable max;

    public NumberBetweenValidatorAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public NumberBetweenValidatorAttribute(double min, double max)
    {
        this.min = min;
        this.max = max;
    }

    public NumberBetweenValidatorAttribute(byte min, byte max)
    {
        this.min = min;
        this.max = max;
    }

    public NumberBetweenValidatorAttribute(short min, short max)
    {
        this.min = min;
        this.max = max;
    }

    public NumberBetweenValidatorAttribute(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

    public NumberBetweenValidatorAttribute(long min, long max)
    {
        this.min = min;
        this.max = max;
    }

    protected override string? OverrideError(object? value)
    {
        if (value == null)
            return null;

        IComparable val = (IComparable)value;

        if (min.GetType() != value.GetType())
        {
            min = (IComparable)Convert.ChangeType(min, val.GetType()); // asi se hace solo una vez
            max = (IComparable)Convert.ChangeType(max, val.GetType());
        }

        if (min.CompareTo(val) <= 0 &&
            val.CompareTo(max) <= 0)
            return null;

        return ValidationMessage._0HasToBeBetween1And2.NiceToString("{0}", min, max);
    }

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        var type = pi.PropertyType.UnNullify();
        return ReflectionTools.IsNumber(type);
    }

    public override string HelpMessage => ValidationMessage.BeBetween0And1.NiceToString(min, max);
}


public class NumberPowerOfTwoValidatorAttribute : ValidatorAttribute
{
    static bool IsPowerOfTwo(long n)
    {
        if (n == 0)
            return false;

        while (n != 1)
        {
            if (n % 2 != 0)
                return false;

            n = n / 2;
        }
        return true;
    }

    protected override string? OverrideError(object? value)
    {
        if (value == null)
            return null;

        if (!IsPowerOfTwo(Convert.ToInt64(value)))
            return ValidationMessage._0ShouldBe12.NiceToString().FormatWith("{0}", ValidationMessage.PowerOf.NiceToString(), 2);

        return null;
    }

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        var type = pi.PropertyType.UnNullify();
        return ReflectionTools.IsIntegerNumber(type);
    }

    public override string HelpMessage => ValidationMessage.Be0.NiceToString(ValidationMessage.PowerOf.NiceToString() + " " + 2);
}

public class NoRepeatValidatorAttribute : ValidatorAttribute
{
    protected override string? OverrideError(object? value)
    {
        IList? list = (IList?)value;
        if (list == null || list.Count <= 1)
            return null;
        string ex = list.Cast<object>().GroupCount().Where(kvp => kvp.Value > 1).ToString(e => "{0} x {1}".FormatWith(e.Key, e.Value), ", ");
        if (ex.HasText())
            return ValidationMessage._0HasSomeRepeatedElements1.NiceToString("{0}", ex);
        return null;
    }

    public override string HelpMessage => ValidationMessage.HaveNoRepeatedElements.NiceToString();

    public static string? ByKey<T, K>(IEnumerable<T> collection, Func<T, K> keySelector)
    {
        var errors = collection.GroupBy(keySelector)
            .Select(gr => new { gr.Key, Count = gr.Count() })
            .Where(a => a.Count > 1)
            .ToString(e => "{0} x {1}".FormatWith(e.Key, e.Count), ", ");

        return errors.DefaultText(null!);
    }

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        return pi.PropertyType.IsMList();
    }
}

public class CountIsValidatorAttribute : ValidatorAttribute
{
    public ComparisonType ComparisonType;
    public int Number;

    public CountIsValidatorAttribute(ComparisonType comparison, int number)
    {
        this.ComparisonType = comparison;
        this.Number = number;
    }

    protected override string? OverrideError(object? value)
    {
        IList? list = (IList?)value;

        int val = list == null ? 0 : list.Count;

        if ((ComparisonType == ComparisonType.EqualTo && val.CompareTo(Number) == 0) ||
            (ComparisonType == ComparisonType.DistinctTo && val.CompareTo(Number) != 0) ||
            (ComparisonType == ComparisonType.GreaterThan && val.CompareTo(Number) > 0) ||
            (ComparisonType == ComparisonType.GreaterThanOrEqualTo && val.CompareTo(Number) >= 0) ||
            (ComparisonType == ComparisonType.LessThan && val.CompareTo(Number) < 0) ||
            (ComparisonType == ComparisonType.LessThanOrEqualTo && val.CompareTo(Number) <= 0))
            return null;

        return ValidationMessage.TheNumberOfElementsOf0HasToBe12.NiceToString().FormatWith("{0}", ComparisonType.NiceToString().FirstLower(), Number.ToString());
    }

    public bool IsGreaterThanZero =>
        ComparisonType == ComparisonType.GreaterThan && Number == 0 ||
        ComparisonType == ComparisonType.GreaterThanOrEqualTo && Number == 1;

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        return pi.PropertyType.IsMList();
    }

    public override string HelpMessage => ValidationMessage.HaveANumberOfElements01.NiceToString().FormatWith(ComparisonType.NiceToString().FirstLower(), Number.ToString());
}

[InTypeScript(true)]
[DescriptionOptions(DescriptionOptions.Members)]
public enum ComparisonType
{
    EqualTo,
    DistinctTo,
    GreaterThan,
    GreaterThanOrEqualTo,
    LessThan,
    LessThanOrEqualTo,
}

public class DateTimePrecisionValidatorAttribute : ValidatorAttribute
{
    public DateTimePrecision Precision { get; private set; }

    public DateTimePrecisionValidatorAttribute(DateTimePrecision precision)
    {
        this.Precision = precision;
    }

    protected override string? OverrideError(object? value)
    {
        if (value == null)
            return null;


        var dto = ToDateTime(value);

        var prec = dto.GetPrecision();
        if (prec > Precision)
            return ValidationMessage._0HasAPrecisionOf1InsteadOf2.NiceToString("{0}", prec, Precision);

        return null;
    }

    public string FormatString
    {
        get
        {
            var dtfi = CultureInfo.CurrentCulture.DateTimeFormat;
            switch (Precision)
            {
                case DateTimePrecision.Days: return "d";
                case DateTimePrecision.Hours: return dtfi.ShortDatePattern + " " + "HH";
                case DateTimePrecision.Minutes: return "g";
                case DateTimePrecision.Seconds: return "G";
                case DateTimePrecision.Milliseconds: return dtfi.ShortDatePattern + " " + dtfi.LongTimePattern.Replace("ss", "ss.fff");
                default: return "";
            }
        }
    }

    internal static DateTime ToDateTime(object? value)
    {
        return value is DateTime dt ? dt :
        value is DateOnly d ? d.ToDateTime() :
            value is DateTimeOffset dto ? dto.DateTime :
            throw new UnexpectedValueException(value);
    }

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        return ReflectionTools.IsDate(pi.PropertyType.UnNullify());
    }

    public override string HelpMessage => ValidationMessage.HaveAPrecisionOf0.NiceToString(Precision.NiceToString().ToLower());
}

public class DateInPastValidatorAttribute : ValidatorAttribute
{
    protected override string? OverrideError(object? value)
    {
        if (value == null)
            return null;

        DateTime dateTime = DateTimePrecisionValidatorAttribute.ToDateTime(value);

        if (dateTime > Clock.Now)
            return ValidationMessage._0ShouldBeADateInThePast.NiceToString();

        return null;
    }

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        return ReflectionTools.IsDate(pi.PropertyType.UnNullify());
    }

    public override string HelpMessage => ValidationMessage.BeInThePast.NiceToString();
}

public class YearGreaterThanValidatorAttribute : ValidatorAttribute
{
    public int MinYear { get; set; }

    public YearGreaterThanValidatorAttribute(int minYear)
    {
        this.MinYear = minYear;
    }

    protected override string? OverrideError(object? value)
    {
        if (value == null)
            return null;

        DateTime dateTime = DateTimePrecisionValidatorAttribute.ToDateTime(value);


        if (dateTime.Year < MinYear)
            return ValidationMessage._0ShouldBe12.NiceToString("{0}", ComparisonType.GreaterThanOrEqualTo.NiceToString(), MinYear);
        return null;
    }

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        return ReflectionTools.IsDate(pi.PropertyType.UnNullify());
    }

    public override string HelpMessage => ValidationMessage.Be0.NiceToString(ComparisonType.GreaterThanOrEqualTo.NiceToString().ToLower() + " " + MinYear);
}


public class TimePrecisionValidatorAttribute : ValidatorAttribute
{
    public DateTimePrecision Precision { get; private set; }

    public TimePrecisionValidatorAttribute(DateTimePrecision precision)
    {
        this.Precision = precision;
    }

    protected override string? OverrideError(object? value)
    {
        if (value == null)
            return null;

        if (value is TimeSpan ts)
        {
            var prec = ts.GetPrecision();
            if (prec > Precision)
                return "{0} has a precision of {1} instead of {2}".FormatWith("{0}", prec, Precision);

            if (ts.Days != 0)
                return "{0} has days";
        }
        else if (value is TimeOnly to)
        {
            var prec = to.GetPrecision();
            if (prec > Precision)
                return "{0} has a precision of {1} instead of {2}".FormatWith("{0}", prec, Precision);
        }

        return null;
    }

    public string FormatString_TimeSpan
    {
        get
        {
            switch (Precision)
            {
                case DateTimePrecision.Hours: return "hh";
                case DateTimePrecision.Minutes: return @"hh\:mm";
                case DateTimePrecision.Seconds: return @"hh\:mm\:ss";
                case DateTimePrecision.Milliseconds: return "c";
                default: return "";
            }
        }
    }

    public string FormatString_TimeOnly
    {
        get
        {
            switch (Precision)
            {
                case DateTimePrecision.Hours: return "HH";
                case DateTimePrecision.Minutes: return @"HH\:mm";
                case DateTimePrecision.Seconds: return @"HH\:mm\:ss";
                case DateTimePrecision.Milliseconds: return "c";
                default: return "";
            }
        }
    }

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        var type = pi.PropertyType.UnNullify();
        return type == typeof(TimeOnly) || type == typeof(TimeSpan);
    }

    public override string HelpMessage => ValidationMessage.HaveAPrecisionOf0.NiceToString(Precision.NiceToString().ToLower());
}

public class StringCaseValidatorAttribute : ValidatorAttribute
{
    private StringCase textCase;
    public StringCase TextCase
    {
        get { return this.textCase; }
        set { this.textCase = value; }
    }

    public StringCaseValidatorAttribute(StringCase textCase)
    {
        this.textCase = textCase;
    }

    protected override string? OverrideError(object? value)
    {
        string? str = (string?)value;
        if (!str.HasText())
            return null;

        if ((this.textCase == StringCase.Uppercase) && (str != str.ToUpper()))
            return ValidationMessage._0HasToBeUppercase.NiceToString();

        if ((this.textCase == StringCase.Lowercase) && (str != str.ToLower()))
            return ValidationMessage._0HasToBeLowercase.NiceToString();

        return null;
    }

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        return pi.PropertyType == typeof(string);
    }

    public override string HelpMessage => ValidationMessage.Be0.NiceToString(textCase.NiceToString());
}

[DescriptionOptions(DescriptionOptions.Members)]
public enum StringCase
{
    Uppercase,
    Lowercase
}



public class IsAssignableToValidatorAttribute : ValidatorAttribute
{
    public Type Type { get; set; }

    public IsAssignableToValidatorAttribute(Type type)
    {
        this.Type = type;
    }

    protected override string? OverrideError(object? value)
    {
        if (value == null)
            return null;

        var t = (TypeEntity)value;
        if (!Type.IsAssignableFrom(t.ToType()))
        {
            return ValidationMessage._0IsNotA1_G.NiceToString().ForGenderAndNumber(Type.GetGender()).FormatWith(t.ToType().NiceName(), Type.NiceName());
        }

        return null;
    }

    public override bool IsCompatibleWith(PropertyInfo pi)
    {
        return pi.PropertyType == typeof(TypeEntity);
    }

    public override string HelpMessage => ValidationMessage.BeA0_G.NiceToString().ForGenderAndNumber(Type.GetGender()).FormatWith(Type.NiceName());
}

public class IpValidatorAttribute : RegexValidatorAttribute
{
    public static Regex IpRegex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

    public IpValidatorAttribute()
        : base(IpRegex)
    {
    }

    public override string FormatName
    {
        get { return "IP"; }
    }
}

public class StateValidator<E, S> : IEnumerable
    where E : ModifiableEntity
    where S : struct
{
    Func<E, S> getState;
    string[] propertyNames;
    PropertyInfo[] properties;
    Func<E, object?>[] getters;

    Dictionary<S, bool?[]> dictionary = new Dictionary<S, bool?[]>();

    public StateValidator(Func<E, S> getState, params Expression<Func<E, object?>>[] propertyGetters)
    {
        this.getState = getState;
        this.properties = propertyGetters.Select(p => ReflectionTools.GetPropertyInfo(p)).ToArray();
        this.propertyNames = this.properties.Select(pi => pi.Name).ToArray();
        this.getters = propertyGetters.Select(p => p.Compile()).ToArray();
    }

    public void Add(S state, params bool?[] necessary)
    {
        if (necessary.Length != propertyNames.Length)
            throw new ArgumentException("The StateValidator {0} for state {1} has {2} values instead of {3}"
                .FormatWith(GetType().TypeName(), state, necessary.Length, propertyNames.Length));

        dictionary.Add(state, necessary);
    }

    public string? Validate(E entity, PropertyInfo pi)
    {
        return Validate(entity, pi, true);
    }

    public bool? IsAllowed(S state, PropertyInfo pi)
    {
        int index = propertyNames.IndexOf(pi.Name);
        if (index == -1)
            return null;

        return Necessary(state, index);
    }

    public string? Validate(E entity, PropertyInfo pi, bool showState)
    {
        int index = propertyNames.IndexOf(pi.Name);
        if (index == -1)
            return null;

        S state = getState(entity);

        return GetMessage(entity, state, showState, index);
    }

    private string? GetMessage(E entity, S state, bool showState, int index)
    {
        bool? necessary = Necessary(state, index);

        if (necessary == null)
            return null;

        object? val = getters[index](entity);
        if (val is IList && ((IList)val).Count == 0 || val is string && ((string)val).Length == 0) //both are indistinguible after retrieving
            val = null;

        if (val != null && !necessary.Value)
            return showState ? ValidationMessage._0IsNotAllowedOnState1.NiceToString().FormatWith(properties[index].NiceName(), state) :
                               ValidationMessage._0IsNotAllowed.NiceToString().FormatWith(properties[index].NiceName());

        if (val == null && necessary.Value)
            return showState ? ValidationMessage._0IsNecessaryOnState1.NiceToString().FormatWith(properties[index].NiceName(), state) :
                               ValidationMessage._0IsNecessary.NiceToString().FormatWith(properties[index].NiceName());

        return null;
    }

    public bool? Necessary(S state, PropertyInfo pi)
    {
        int index = propertyNames.IndexOf(pi.Name);
        if (index == -1)
            throw new ArgumentException("The property is not registered");

        return Necessary(state, index);
    }

    bool? Necessary(S state, int index)
    {
        return dictionary.GetOrThrow(state, "State {0} not registered in StateValidator")[index];
    }

    public IEnumerator GetEnumerator() //just to use object initializer
    {
        throw new NotImplementedException();
    }

    public string? PreviewErrors(E entity, S targetState, bool showState)
    {
        string result = propertyNames.Select((pn, i) => GetMessage(entity, targetState, showState, i)).NotNull().ToString("\n");

        return string.IsNullOrEmpty(result) ? null : result;
    }
}


public enum ValidationMessage
{
    [Description("{0} does not have a valid {1} format")]
    _0DoesNotHaveAValid1Format,
    [Description("'{0}' does not have a valid {1} identifier format")]
    _0DoesNotHaveAValid1IdentifierFormat,
    [Description("{0} has an invalid format")]
    _0HasAnInvalidFormat,
    [Description("{0} has more than {1} decimal places")]
    _0HasMoreThan1DecimalPlaces,
    [Description("{0} has some repeated elements: {1}")]
    _0HasSomeRepeatedElements1,
    [Description("{0} should be {1} {2}")]
    _0ShouldBe12,
    [Description("{0} should be {1}")]
    _0ShouldBe1,
    [Description("{0} should be {1} instead of {2}")]
    _0ShouldBe1InsteadOf2,
    [Description("{0} has to be between {1} and {2}")]
    _0HasToBeBetween1And2,
    [Description("{0} has to be lowercase")]
    _0HasToBeLowercase,
    [Description("{0} has to be uppercase")]
    _0HasToBeUppercase,
    [Description("{0} is necessary")]
    _0IsNecessary,
    [Description("{0} is necessary on state {1}")]
    _0IsNecessaryOnState1,
    [Description("{0} is not allowed")]
    _0IsNotAllowed,
    [Description("{0} is not allowed on state {1}")]
    _0IsNotAllowedOnState1,
    [Description("{0} is not set")]
    _0IsNotSet,
    [Description("{0} is not set in {1}")]
    _0IsNotSetIn1,
    [Description("{0} are not set")]
    _0AreNotSet,
    [Description("{0} is set")]
    _0IsSet,
    [Description("{0} is not a {1}")]
    _0IsNotA1_G,
    [Description("be a {0}")]
    BeA0_G,
    [Description("be {0}")]
    Be0,
    [Description("be between {0} and {1}")]
    BeBetween0And1,
    [Description("be not null")]
    BeNotNull,
    [Description("file name")]
    FileName,
    [Description("have {0} decimals")]
    Have0Decimals,
    [Description("have a number of elements {0} {1}")]
    HaveANumberOfElements01,
    [Description("have a precision of {0}")]
    HaveAPrecisionOf0,
    [Description("have between {0} and {1} characters")]
    HaveBetween0And1Characters,
    [Description("have maximum {0} characters")]
    HaveMaximum0Characters,
    [Description("have minimum {0} characters")]
    HaveMinimum0Characters,
    [Description("have no repeated elements")]
    HaveNoRepeatedElements,
    [Description("have a valid {0} format")]
    HaveValid0Format,
    InvalidDateFormat,
    InvalidFormat,
    [Description("Not possible to assign {0}")]
    NotPossibleToaAssign0,
    Numeric,
    [Description("or be null")]
    OrBeNull,
    Telephone,
    [Description("{0} should have just one line")]
    _0ShouldHaveJustOneLine,
    [Description("{0} should not have initial spaces")]
    _0ShouldNotHaveInitialSpaces,
    [Description("{0} should not have final spaces")]
    _0ShouldNotHaveFinalSpaces,
    [Description("The lenght of {0} has to be equal to {1}")]
    TheLenghtOf0HasToBeEqualTo1,
    [Description("The length of {0} has to be greater than or equal to {1}")]
    TheLengthOf0HasToBeGreaterOrEqualTo1,
    [Description("The length of {0} has to be less than or equal to {1}")]
    TheLengthOf0HasToBeLesserOrEqualTo1,
    [Description("The number of {0} is being multiplied by {1}")]
    TheNumberOf0IsBeingMultipliedBy1,
    [Description("The rows are being grouped by {0}")]
    TheRowsAreBeingGroupedBy0,
    [Description("Each row represents a group of {0} with same {1}")]
    EachRowRepresentsAGroupOf0WithSame1,
    [Description("The number of elements of {0} has to be {1} {2}")]
    TheNumberOfElementsOf0HasToBe12,
    [Description("Type {0} not allowed")]
    Type0NotAllowed,

    [Description("{0} is mandatory when {1} is not set")]
    _0IsMandatoryWhen1IsNotSet,
    [Description("{0} is mandatory when {1} is not set to {2}.")]
    _0IsMandatoryWhen1IsNotSetTo2,
    [Description("{0} is mandatory when {1} is set")]
    _0IsMandatoryWhen1IsSet,
    [Description("{0} is mandatory when {1} is set to {2}.")]
    _0IsMandatoryWhen1IsSetTo2,
    [Description("{0} should be null when {1} is not set")]
    _0ShouldBeNullWhen1IsNotSet,
    [Description("{0} should be null when {1} is not set to {2}.")]
    _0ShouldBeNullWhen1IsNotSetTo2,
    [Description("{0} should be null when {1} is set")]
    _0ShouldBeNullWhen1IsSet,
    [Description("{0} should be null when {1} is set to {2}.")]
    _0ShouldBeNullWhen1IsSetTo2,
    [Description("{0} should be null")]
    _0ShouldBeNull,
    [Description("{0} should be {1} when {2} is {3}")]
    _0ShouldBe1When2Is3,
    [Description("{0} should be a date in the past")]
    _0ShouldBeADateInThePast,
    [Description("{0} should be a date in the future")]
    _0ShouldBeADateInTheFuture,
    BeInThePast,
    [Description("{0} should be greater than {1}")]
    _0ShouldBeGreaterThan1,
    [Description("{0} should be greater than or equal {1}")]
    _0ShouldBeGreaterThanOrEqual1,
    [Description("{0} should be less than {1}")]
    _0ShouldBeLessThan1,
    [Description("{0} should be less than or equal {1}")]
    _0ShouldBeLessThanOrEqual1,
    [Description("{0} has a precision of {1} instead of {2}")]
    _0HasAPrecisionOf1InsteadOf2,
    [Description("{0} should be of type {1}")]
    _0ShouldBeOfType1,
    [Description("{0} should not be of type {1}")]
    _0ShouldNotBeOfType1,
    [Description("{0} and {1} can not be set at the same time")]
    _0And1CanNotBeSetAtTheSameTime,
    [Description("{0} or {1} should be set")]
    _0Or1ShouldBeSet,
    [Description("{0} and {1} and {2} can not be set at the same time")]
    _0And1And2CanNotBeSetAtTheSameTime,
    [Description("{0} have {1} elements, but allowed only {2}")]
    _0Have1ElementsButAllowedOnly2,
    [Description("{0} is empty")]
    _0IsEmpty,
    [Description("{0} should be empty")]
    _0ShouldBeEmpty,
    [Description("At least one value is needed")]
    _AtLeastOneValueIsNeeded,
    PowerOf,
    BeAString,
    BeAMultilineString,
    IsATimeOfTheDay,

    [Description("There are {0} in state {1}")]
    ThereAre0InState1,
    [Description("There are {0} that reference this {1}")]
    ThereAre0ThatReferenceThis1,

    [Description("{0} is not compatible with {1}")]
    _0IsNotCompatibleWith1,
    [Description("{0} is repeated")]
    _0IsRepeated,

    NumberIsTooBig,
    NumberIsTooSmall,
}

public static class ValidationMessageHelper
{
    public static string ShouldBe(this PropertyInfo pi, ComparisonType ct, object value)
    {
        return ValidationMessage._0ShouldBe12.NiceToString(pi.NiceName(), ct.NiceToString(),
            value is PropertyInfo pi2 ? pi2.NiceName() :
            value is Type t ? t.NiceName() :
            value is Enum e ? e.NiceToString() :
            value is null ? "null" :
            value?.ToString());
    }
}
