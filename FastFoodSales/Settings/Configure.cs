using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DAQ
{
    [DebuggerDisplay("{" + nameof(FullPath) + "}")]
    public class FileLocator : IEquatable<FileLocator>
    {
        /// <summary>
        /// Gets a regular expression for splitting the file full path string.
        /// In the right case, the group will have four elements:
        /// [0]: FullPath
        /// [1]: FolderName
        /// [2]: FileName
        /// [3]: FileExtension
        /// </summary>
        private static readonly Regex RegexFileLocation = new Regex(@"^([\\/]?(?:\w:)?(?:[^\\/]+?[\\/])*?)([^\\/]+?(?:\.(\w+?))?)?$", RegexOptions.Compiled);


        /// <summary>
        /// Gets a string representing the full path of the file. 
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Gets a string representing the folder where the file is located. 
        /// </summary>
        public string FolderPath { get; }

        /// <summary>
        /// Gets a string representing the file name. 
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets a string representing the file extension. 
        /// </summary>
        public string FileExtension { get; }

        /// <summary>
        /// Initializes a instance of <see cref="FileLocator"/> with specified file full path.
        /// </summary>
        /// <param name="fileFullPath">A string representing the full path of the file. </param>
        public FileLocator(string fileFullPath)
        {
            var matchResult = RegexFileLocation.Match(fileFullPath);

            if (matchResult.Groups == null || matchResult.Groups.Count != 4)
                throw new ArgumentException($"The file path is not valid: {fileFullPath}", fileFullPath);

            FullPath = matchResult.Groups[0].Value;
            var temp = matchResult.Groups[1].Value;
            if (!string.IsNullOrEmpty(temp)) FolderPath = temp.Remove(temp.Length - 1); // Remove the "\" or "/" at the end. 
            FileName = matchResult.Groups[2].Value;
            FileExtension = matchResult.Groups[3].Value.ToLower();
        }

        public override string ToString() => FullPath;

        #region Implements Equals

        public bool Equals(FileLocator other) => !Equals(other, null) && string.Equals(FullPath, other.FullPath);

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || Equals(obj as FileLocator);

        public static bool operator ==(FileLocator left, FileLocator right)
        {
            if ((object)left == null || (object)right == null)
                return Equals(left, right);

            return left.Equals(right);
        }

        public static bool operator !=(FileLocator left, FileLocator right) => !(left == right);

        public override int GetHashCode() => FullPath != null ? FullPath.GetHashCode() : 0;

        #endregion

        public static implicit operator FileLocator(string filePath) => string.IsNullOrEmpty(filePath) ? null : new FileLocator(filePath);

        public static implicit operator string(FileLocator fileLocation) => fileLocation?.FullPath;
    }
    public class FileLocatorConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var fileLocator = (FileLocator)value;
            writer.WriteValue(fileLocator.FullPath);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(FileLocator);

    }

    public static class JsonExtensions
    {
        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
        public static readonly JsonSerializerSettings JsonDeserializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
        public static readonly FileLocatorConverter FileLocatorConverter = new FileLocatorConverter();

        static JsonExtensions()
        {
            JsonSerializerSettings.Converters.Add(FileLocatorConverter);
        }

        public static string ToJson<T>(this T @object, Formatting formatting = Formatting.None)
        {
            var type = @object.GetType();

            return typeof(T) != type
                ? JsonConvert.SerializeObject(@object, typeof(T), formatting, JsonSerializerSettings)
                : JsonConvert.SerializeObject(@object, formatting, JsonSerializerSettings);
        }

        public static T ToObject<T>(this string json)
        {
            return !string.IsNullOrWhiteSpace(json)
                ? JsonConvert.DeserializeObject<T>(json, JsonDeserializerSettings)
                : default;
        }


    }
    public class ValueChangedEventArgs : EventArgs
    {
        public string KeyName { get; }

        public ValueChangedEventArgs(string keyName) => KeyName = keyName;
    }

    public interface IConfigureFile
    {
        event EventHandler<ValueChangedEventArgs> ValueChanged;

        bool Contains(string key);

        T GetValue<T>(string key);

        IConfigureFile SetValue<T>(string key, T value);

        IConfigureFile Load(string filePath = null);

        IConfigureFile Clear();

        void Delete();
    }
    public static class AppFiles
    {
        /// <summary>
        /// %AppData%\MV\MV.config
        /// </summary>
        public static readonly string Configure = Path.Combine(AppFolders.AppData, "MV.config");
    }

    public static partial class AppFolders
    {
        static AppFolders()
        {
            Directory.CreateDirectory(Apps);
            Directory.CreateDirectory(Logs);
            Directory.CreateDirectory(Users);
            Directory.CreateDirectory(Drivers);
            Directory.CreateDirectory(Modules);

        }

        /// <summary>
        /// It represents the path where the "MV.exe" is located.
        /// </summary>
        public static readonly string MainProgram = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// %AppData%\MV
        /// </summary>
        public static readonly string AppData = Path.Combine(MainProgram, "Configs");

        /// <summary>
        /// %AppData%\MV\Apps
        /// </summary>
        public static readonly string Apps = Path.Combine(AppData, nameof(Apps));

        /// <summary>
        /// %AppData%\MV\Logs
        /// </summary>
        public static readonly string Logs = Path.Combine(AppData, nameof(Logs));
        /// <summary>
        /// %AppData%\MV\Users
        /// </summary>
        public static readonly string Users = Path.Combine(AppData, nameof(Users));
        public static readonly string Drivers = Path.Combine(MainProgram, nameof(Drivers));
        public static readonly string Modules = Path.Combine(MainProgram, nameof(Modules));
    }
    public class ConfigureFile : IConfigureFile
    {
        private JObject _storage;
        private string _filePath = AppFiles.Configure;

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        public bool Contains(string key) => _storage.Values().Any(token => token.Path == key);

        public T GetValue<T>(string key)
        {

            var result = (_storage[key]?.ToString() ?? string.Empty).ToObject<T>();
            if (result == null && typeof(T).GetConstructors().Any(x => !x.GetParameters().Any()))
            {
                result = (T)Activator.CreateInstance(typeof(T));
                _storage[key] = result.ToJson(Formatting.Indented);
                Save();
                ValueChanged?.Invoke(this, new ValueChangedEventArgs(key));
            }
            return result;
        }


        public IConfigureFile SetValue<T>(string key, T value)
        {
            if (EqualityComparer<T>.Default.Equals(GetValue<T>(key), value)) return this;

            _storage[key] = value.ToJson(Formatting.None);
            Save();
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(key));
            return this;
        }

        public IConfigureFile Load(string filePath = null)
        {
            if (!string.IsNullOrEmpty(filePath)) _filePath = filePath;

            if (!File.Exists(_filePath))
            {
                _storage = new JObject(JObject.Parse("{}"));
                Save();
            }
            _storage = JObject.Parse(File.ReadAllText(_filePath));

            return this;
        }

        public IConfigureFile Clear()
        {
            _storage = new JObject();
            Save();
            return this;
        }

        public void Delete()
        {
            Clear();
            File.Delete(_filePath);
        }


        private void Save() => WriteToLocal(_filePath, _storage.ToString(Formatting.Indented));

        private void WriteToLocal(string path, string text)
        {
            try
            {
                File.WriteAllText(path, text);
            }
            catch (IOException)
            {
                WriteToLocal(path, text);
            }
        }
    }
}