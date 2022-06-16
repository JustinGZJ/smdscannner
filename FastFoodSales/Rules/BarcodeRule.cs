using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DAQ.Rules
{
    public class BarcodeRule : ValidationRule
    {

        string[] barcodes = new string[] { "BASE-ERX1815/CE-8P+LT (LOTES)", "BASE-ERX1815/CE-8P+EVW (EVERWIN)", "BASE-ERX1815/CE-8P+LY (LYT)" };
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
                return new ValidationResult(false, "不能为空值！");
            if (string.IsNullOrEmpty(value.ToString()))
                return new ValidationResult(false, "不能为空字符串！");
            if (value is string v)
            {
                if (!barcodes.Any(x => v.Contains(x)))
                {
                    return new ValidationResult(false, "数据不匹配");
                }
            }
            return new ValidationResult(true, null);
        }
    }
    public class AppConfig
    {
        public string[] BobinPartRule { get; set; } = new string[] { "BASE-PTS2817/CE-4P+LT" };
    }



    public class BobinPartNameRule : ValidationRule
    {
        IConfigureFile config;
        public BobinPartNameRule()
        {

            config = new ConfigureFile().Load();
            if (config.GetValue<AppConfig>(nameof(AppConfig)) == null)
            {
                config.SetValue<AppConfig>(nameof(AppConfig), new AppConfig());
            }
            this.barcodes = config.GetValue<AppConfig>(nameof(AppConfig)).BobinPartRule;
            //  config.GetValue<>
        }

        string[] barcodes = new string[] { "BASE-PTS2817/CE-4P+LT" };
        private readonly IConfigureFile configure;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
                return new ValidationResult(false, "不能为空值！");
            if (string.IsNullOrEmpty(value.ToString()))
                return new ValidationResult(false, "不能为空字符串！");
            if (value is string v)
            {
                if (!barcodes.Any(x => v.Contains(x)))
                {
                    return new ValidationResult(false, $"数据不匹配,{string.Join(",", barcodes)}");
                }
            }
            return new ValidationResult(true, null);
        }
    }
}