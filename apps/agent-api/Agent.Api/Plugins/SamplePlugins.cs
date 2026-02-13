namespace Agent.Api.Plugins;

/// <summary>
/// Sample plugin for mathematical operations
/// 数学运算示例插件
/// </summary>
public class MathPlugin
{
    /// <summary>
    /// Add two numbers
    /// 两数相加
    /// </summary>
    /// <param name="a">First number - 第一个数</param>
    /// <param name="b">Second number - 第二个数</param>
    /// <returns>Sum of the two numbers - 两数之和</returns>
    [KernelFunction, Description("Add two numbers together")]
    public double Add(
        [Description("The first number to add")] double a,
        [Description("The second number to add")] double b)
    {
        return a + b;
    }

    /// <summary>
    /// Subtract two numbers
    /// 两数相减
    /// </summary>
    /// <param name="a">First number - 被减数</param>
    /// <param name="b">Second number - 减数</param>
    /// <returns>Difference of the two numbers - 两数之差</returns>
    [KernelFunction, Description("Subtract the second number from the first")]
    public double Subtract(
        [Description("The number to subtract from")] double a,
        [Description("The number to subtract")] double b)
    {
        return a - b;
    }

    /// <summary>
    /// Multiply two numbers
    /// 两数相乘
    /// </summary>
    /// <param name="a">First number - 第一个数</param>
    /// <param name="b">Second number - 第二个数</param>
    /// <returns>Product of the two numbers - 两数之积</returns>
    [KernelFunction, Description("Multiply two numbers together")]
    public double Multiply(
        [Description("The first number to multiply")] double a,
        [Description("The second number to multiply")] double b)
    {
        return a * b;
    }

    /// <summary>
    /// Divide two numbers
    /// 两数相除
    /// </summary>
    /// <param name="a">Dividend - 被除数</param>
    /// <param name="b">Divisor - 除数</param>
    /// <returns>Quotient of the two numbers - 两数之商</returns>
    [KernelFunction, Description("Divide the first number by the second")]
    public double Divide(
        [Description("The number to be divided")] double a,
        [Description("The number to divide by")] double b)
    {
        if (b == 0)
        {
            throw new ArgumentException("Cannot divide by zero - 不能除以零");
        }
        return a / b;
    }

    /// <summary>
    /// Calculate the power of a number
    /// 计算数的幂
    /// </summary>
    /// <param name="baseNumber">Base number - 底数</param>
    /// <param name="exponent">Exponent - 指数</param>
    /// <returns>Result of base raised to the power of exponent - 底数的指数次幂</returns>
    [KernelFunction, Description("Calculate the power of a number")]
    public double Power(
        [Description("The base number")] double baseNumber,
        [Description("The exponent")] double exponent)
    {
        return Math.Pow(baseNumber, exponent);
    }

    /// <summary>
    /// Calculate the square root of a number
    /// 计算数的平方根
    /// </summary>
    /// <param name="number">Number to calculate square root - 要计算平方根的数</param>
    /// <returns>Square root of the number - 数的平方根</returns>
    [KernelFunction, Description("Calculate the square root of a number")]
    public double SquareRoot(
        [Description("The number to calculate square root of")] double number)
    {
        if (number < 0)
        {
            throw new ArgumentException("Cannot calculate square root of negative number - 不能计算负数的平方根");
        }
        return Math.Sqrt(number);
    }

    /// <summary>
    /// Calculate the area of a circle
    /// 计算圆的面积
    /// </summary>
    /// <param name="radius">Radius of the circle - 圆的半径</param>
    /// <returns>Area of the circle - 圆的面积</returns>
    [KernelFunction, Description("Calculate the area of a circle given its radius")]
    public double CircleArea(
        [Description("The radius of the circle")] double radius)
    {
        if (radius < 0)
        {
            throw new ArgumentException("Radius cannot be negative - 半径不能为负数");
        }
        return Math.PI * radius * radius;
    }

    /// <summary>
    /// Calculate the area of a rectangle
    /// 计算矩形的面积
    /// </summary>
    /// <param name="length">Length of the rectangle - 矩形的长</param>
    /// <param name="width">Width of the rectangle - 矩形的宽</param>
    /// <returns>Area of the rectangle - 矩形的面积</returns>
    [KernelFunction, Description("Calculate the area of a rectangle")]
    public double RectangleArea(
        [Description("The length of the rectangle")] double length,
        [Description("The width of the rectangle")] double width)
    {
        if (length < 0 || width < 0)
        {
            throw new ArgumentException("Length and width cannot be negative - 长和宽不能为负数");
        }
        return length * width;
    }

    /// <summary>
    /// Calculate the factorial of a number
    /// 计算数的阶乘
    /// </summary>
    /// <param name="n">Number to calculate factorial - 要计算阶乘的数</param>
    /// <returns>Factorial of the number - 数的阶乘</returns>
    [KernelFunction, Description("Calculate the factorial of a non-negative integer")]
    public long Factorial(
        [Description("The non-negative integer to calculate factorial of")] int n)
    {
        if (n < 0)
        {
            throw new ArgumentException("Cannot calculate factorial of negative number - 不能计算负数的阶乘");
        }

        if (n == 0 || n == 1)
        {
            return 1;
        }

        long result = 1;
        for (int i = 2; i <= n; i++)
        {
            result *= i;
        }

        return result;
    }

    /// <summary>
    /// Check if a number is prime
    /// 检查数是否为质数
    /// </summary>
    /// <param name="number">Number to check - 要检查的数</param>
    /// <returns>True if the number is prime, false otherwise - 如果是质数返回true，否则返回false</returns>
    [KernelFunction, Description("Check if a number is prime")]
    public bool IsPrime(
        [Description("The number to check for primality")] int number)
    {
        if (number < 2)
        {
            return false;
        }

        if (number == 2)
        {
            return true;
        }

        if (number % 2 == 0)
        {
            return false;
        }

        for (int i = 3; i * i <= number; i += 2)
        {
            if (number % i == 0)
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// Sample plugin for text operations
/// 文本操作示例插件
/// </summary>
public class TextPlugin
{
    /// <summary>
    /// Convert text to uppercase
    /// 将文本转换为大写
    /// </summary>
    /// <param name="text">Text to convert - 要转换的文本</param>
    /// <returns>Uppercase text - 大写文本</returns>
    [KernelFunction, Description("Convert text to uppercase")]
    public string ToUpperCase(
        [Description("The text to convert to uppercase")] string text)
    {
        return text?.ToUpper() ?? string.Empty;
    }

    /// <summary>
    /// Convert text to lowercase
    /// 将文本转换为小写
    /// </summary>
    /// <param name="text">Text to convert - 要转换的文本</param>
    /// <returns>Lowercase text - 小写文本</returns>
    [KernelFunction, Description("Convert text to lowercase")]
    public string ToLowerCase(
        [Description("The text to convert to lowercase")] string text)
    {
        return text?.ToLower() ?? string.Empty;
    }

    /// <summary>
    /// Count the number of words in text
    /// 计算文本中的单词数
    /// </summary>
    /// <param name="text">Text to count words in - 要计算单词数的文本</param>
    /// <returns>Number of words - 单词数</returns>
    [KernelFunction, Description("Count the number of words in text")]
    public int CountWords(
        [Description("The text to count words in")] string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return text.Split(new char[] { ' ', '\t', '\n', '\r' },
            StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Reverse the text
    /// 反转文本
    /// </summary>
    /// <param name="text">Text to reverse - 要反转的文本</param>
    /// <returns>Reversed text - 反转后的文本</returns>
    [KernelFunction, Description("Reverse the order of characters in text")]
    public string ReverseText(
        [Description("The text to reverse")] string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        char[] charArray = text.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    /// <summary>
    /// Extract email addresses from text
    /// 从文本中提取电子邮件地址
    /// </summary>
    /// <param name="text">Text to extract emails from - 要提取邮件的文本</param>
    /// <returns>Comma-separated list of email addresses - 逗号分隔的邮件地址列表</returns>
    [KernelFunction, Description("Extract email addresses from text")]
    public string ExtractEmails(
        [Description("The text to extract email addresses from")] string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
        var matches = System.Text.RegularExpressions.Regex.Matches(text, emailPattern);

        return string.Join(", ", matches.Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Value));
    }
}

/// <summary>
/// Sample plugin for date and time operations
/// 日期时间操作示例插件
/// </summary>
public class DateTimePlugin
{
    /// <summary>
    /// Get current date and time
    /// 获取当前日期和时间
    /// </summary>
    /// <returns>Current date and time - 当前日期和时间</returns>
    [KernelFunction, Description("Get the current date and time")]
    public string GetCurrentDateTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Get current date
    /// 获取当前日期
    /// </summary>
    /// <returns>Current date - 当前日期</returns>
    [KernelFunction, Description("Get the current date")]
    public string GetCurrentDate()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Get current time
    /// 获取当前时间
    /// </summary>
    /// <returns>Current time - 当前时间</returns>
    [KernelFunction, Description("Get the current time")]
    public string GetCurrentTime()
    {
        return DateTime.Now.ToString("HH:mm:ss");
    }

    /// <summary>
    /// Add days to a date
    /// 在日期上添加天数
    /// </summary>
    /// <param name="dateString">Date in YYYY-MM-DD format - YYYY-MM-DD格式的日期</param>
    /// <param name="days">Number of days to add - 要添加的天数</param>
    /// <returns>New date after adding days - 添加天数后的新日期</returns>
    [KernelFunction, Description("Add days to a date")]
    public string AddDays(
        [Description("The date in YYYY-MM-DD format")] string dateString,
        [Description("The number of days to add")] int days)
    {
        if (DateTime.TryParse(dateString, out DateTime date))
        {
            return date.AddDays(days).ToString("yyyy-MM-dd");
        }

        throw new ArgumentException("Invalid date format. Use YYYY-MM-DD - 无效的日期格式，请使用YYYY-MM-DD");
    }

    /// <summary>
    /// Calculate days between two dates
    /// 计算两个日期之间的天数
    /// </summary>
    /// <param name="startDate">Start date in YYYY-MM-DD format - YYYY-MM-DD格式的开始日期</param>
    /// <param name="endDate">End date in YYYY-MM-DD format - YYYY-MM-DD格式的结束日期</param>
    /// <returns>Number of days between the dates - 两个日期之间的天数</returns>
    [KernelFunction, Description("Calculate the number of days between two dates")]
    public int DaysBetween(
        [Description("The start date in YYYY-MM-DD format")] string startDate,
        [Description("The end date in YYYY-MM-DD format")] string endDate)
    {
        if (DateTime.TryParse(startDate, out DateTime start) &&
            DateTime.TryParse(endDate, out DateTime end))
        {
            return (int)(end - start).TotalDays;
        }

        throw new ArgumentException("Invalid date format. Use YYYY-MM-DD - 无效的日期格式，请使用YYYY-MM-DD");
    }

    /// <summary>
    /// Get day of week for a date
    /// 获取日期是星期几
    /// </summary>
    /// <param name="dateString">Date in YYYY-MM-DD format - YYYY-MM-DD格式的日期</param>
    /// <returns>Day of the week - 星期几</returns>
    [KernelFunction, Description("Get the day of the week for a date")]
    public string GetDayOfWeek(
        [Description("The date in YYYY-MM-DD format")] string dateString)
    {
        if (DateTime.TryParse(dateString, out DateTime date))
        {
            return date.DayOfWeek.ToString();
        }

        throw new ArgumentException("Invalid date format. Use YYYY-MM-DD - 无效的日期格式，请使用YYYY-MM-DD");
    }
}

