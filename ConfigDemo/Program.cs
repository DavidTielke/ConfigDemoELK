using System.Reflection;

[assembly:AssemblyVersion("1.0.0.1")]

namespace ConfigDemo
{

    internal class Program
    {
        static void Main(string[] args)
        {
            var current = AppDomain.CurrentDomain;
            current.UnhandledException += UnhandledException;

            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version);


            var config = new Configurator();

            var configObject = new FooConfig()
            {
                Id = 1,
                Name = "David",
                Age = 39
            };


            config.SetConfigObject("Camera1", configObject);

            var config1 = config.GetConfigObject<FooConfig>("Camera1");

            configObject.Name = "Philipp";

            config.ResetToDefaultValue("Camera1.Id");

            config.SetConfigObject("Camera1", configObject);

            var config2 = config.GetConfigObject<FooConfig>("Camera1");
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //if (e.ExceptionObject is CameraException)
            //{

            //}
        }
    }

    class FooConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    internal interface IConfigurator
    {
        void Set(string key, object value);
        T Get<T>(string key);
    }

    class Configurator : IConfigurator
    {
        private readonly Dictionary<string, object> _items;
        private readonly Dictionary<string, object> _defaultValues;

        public Configurator()
        {
            _defaultValues = new Dictionary<string, object>();

            InitialDefaultValues();

            _items = _defaultValues.ToDictionary();
        }

        private void InitialDefaultValues()
        {
            // hier aus JsonConfig laden
            _defaultValues["Camera1.Id"] = 4711;
        }

        public void ResetToDefaultValue(string key)
        {
            _items[key] = _defaultValues[key];
        }

        public void SetConfigObject(string key, object config)
        {
            var properties = config.GetType().GetProperties();
            foreach (var property in properties)
            {
                Set(key+"."+property.Name, property.GetValue(config));
            }
        }

        public TConfig GetConfigObject<TConfig>(string key)
        {
            var configObject = Activator.CreateInstance<TConfig>();

            var properties = configObject.GetType().GetProperties();
            foreach (var property in properties)
            {
                var propName = property.Name;
                var storedValue = _items[key+"."+propName];
                property.SetValue(configObject, storedValue);
            }

            return configObject;
        }

        public void Set(string key, object value)
        {
            var isAlreadySet = _items.ContainsKey(key);
            if (isAlreadySet)
            {
                var oldValue = _items[key];
                var newValue = value;
                if (!oldValue.Equals(newValue))
                {
                    // Eventaggregator nutzen
                    Console.WriteLine($"Geändert: {key}: {value}");
                }
            }

            _items[key] = value;
        }

        public T Get<T>(string key)
        {
            return (T)_items[key];
        }
    }
}
